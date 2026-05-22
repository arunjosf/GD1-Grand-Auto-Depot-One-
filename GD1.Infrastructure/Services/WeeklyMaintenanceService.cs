using GD1.Domain.Entities;
using GD1.Domain.Entities.Enums;
using GD1.Domain.Interfaces;
using GD1.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Infrastructure.Services
{
    public class WeeklyMaintenanceService : BackgroundService
    {
        private readonly ILogger<WeeklyMaintenanceService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public WeeklyMaintenanceService(ILogger<WeeklyMaintenanceService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessWeeklyTasksAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing WeeklyMaintenanceService.");
                }

                // Run once every 24 hours
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }

        private async Task ProcessWeeklyTasksAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Find all vehicles currently stored in a lot
            var storedBookings = await dbContext.Bookings
                .Include(b => b.Property)
                .ThenInclude(p => p.Managers)
                .Where(b => b.Status == BookingStatus.InLot)
                .ToListAsync(stoppingToken);

            foreach (var booking in storedBookings)
            {
                var manager = booking.Property?.Managers?.FirstOrDefault();
                if (manager == null) continue;

                // Check the last time a WeeklyConditionCheck was completed for this vehicle
                var lastWeeklyTask = await dbContext.MaintenanceTasks
                    .Where(t => t.VehicleId == booking.VehicleId && t.Type == MaintenanceTaskType.WeeklyConditionCheck)
                    .OrderByDescending(t => t.RequestedAt)
                    .FirstOrDefaultAsync(stoppingToken);

                bool needsUpdate = false;

                if (lastWeeklyTask == null)
                {
                    // Check if it's been 7 days since the vehicle arrived
                    var arrivalEvent = await dbContext.VehicleJourneyEvents
                        .Where(e => e.VehicleId == booking.VehicleId && e.EventType == "VehicleStored")
                        .OrderByDescending(e => e.CreatedAt)
                        .FirstOrDefaultAsync(stoppingToken);

                    if (arrivalEvent != null && (DateTime.UtcNow - arrivalEvent.CreatedAt).TotalDays >= 7)
                    {
                        needsUpdate = true;
                    }
                }
                else if ((DateTime.UtcNow - lastWeeklyTask.RequestedAt).TotalDays >= 7)
                {
                    // It's been 7 days since the last weekly task was requested
                    needsUpdate = true;
                }

                if (needsUpdate)
                {
                    // Check if one is already pending to avoid duplicates
                    var isPending = await dbContext.MaintenanceTasks
                        .AnyAsync(t => t.VehicleId == booking.VehicleId && t.Type == MaintenanceTaskType.WeeklyConditionCheck && t.Status == MaintenanceTaskStatus.Pending, stoppingToken);

                    if (!isPending)
                    {
                        var task = new MaintenanceTask
                        {
                            VehicleId = booking.VehicleId,
                            BookingId = booking.Id,
                            ManagerId = manager.Id,
                            Type = MaintenanceTaskType.WeeklyConditionCheck,
                            Status = MaintenanceTaskStatus.Pending,
                            RequestedAt = DateTime.UtcNow
                        };

                        dbContext.MaintenanceTasks.Add(task);
                        _logger.LogInformation("Created WeeklyConditionCheck for Vehicle {VehicleId}", booking.VehicleId);
                    }
                }
            }

            await dbContext.SaveChangesAsync(stoppingToken);
        }
    }
}

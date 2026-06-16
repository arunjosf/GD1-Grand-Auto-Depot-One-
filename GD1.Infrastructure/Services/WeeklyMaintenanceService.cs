using GD1.Domain.Entities;
using GD1.Domain.Entities.Enums;
using GD1.Domain.Interfaces;
using GD1.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using GD1.Application.Interfaces.Services;
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

                try
                {
                    // Run once every 24 hours
                    await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break; // App shutting down
                }
            }
        }

        private async Task ProcessWeeklyTasksAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            // Find all vehicles currently stored in a lot
            var storedBookings = await dbContext.Bookings
                .Include(b => b.Vehicle)
                .Include(b => b.Property)
                .ThenInclude(p => p.Managers)
                .Where(b => b.Status == BookingStatus.InLot)
                .ToListAsync(stoppingToken);

            foreach (var booking in storedBookings)
            {
                var manager = booking.Property?.Managers?.FirstOrDefault();
                if (manager == null) continue;

                // Check the last time a weekly task was requested by the system
                var lastWeeklyTask = await dbContext.MaintenanceTasks
                    .Where(t => t.VehicleId == booking.VehicleId && t.Type == MaintenanceTaskType.WeeklyConditionCheck)
                    .OrderByDescending(t => t.RequestedAt)
                    .FirstOrDefaultAsync(stoppingToken);

                // Check the last time an actual update was submitted (either via task or adhoc)
                var lastActualUpdate = await dbContext.VehicleJourneyEvents
                    .Where(e => e.VehicleId == booking.VehicleId && (e.EventType == "WeeklyUpdate" || e.EventType == "AdHocMaintenanceUpdate"))
                    .OrderByDescending(e => e.CreatedAt)
                    .FirstOrDefaultAsync(stoppingToken);

                bool needsUpdate = false;

                // The reference date is the most recent of either the last task requested or the last actual update submitted.
                // If neither exists, use the vehicle arrival date.
                DateTime? referenceDate = null;
                
                if (lastActualUpdate != null)
                {
                    referenceDate = lastActualUpdate.CreatedAt;
                }
                
                if (lastWeeklyTask != null && (referenceDate == null || lastWeeklyTask.RequestedAt > referenceDate))
                {
                    referenceDate = lastWeeklyTask.RequestedAt;
                }

                if (referenceDate == null)
                {
                    // Check if it's been 7 days since the vehicle arrived
                    var arrivalEvent = await dbContext.VehicleJourneyEvents
                        .Where(e => e.VehicleId == booking.VehicleId && e.EventType == "VehicleStored")
                        .OrderByDescending(e => e.CreatedAt)
                        .FirstOrDefaultAsync(stoppingToken);

                    if (arrivalEvent != null)
                    {
                        referenceDate = arrivalEvent.CreatedAt;
                    }
                }

                if (referenceDate.HasValue && (DateTime.UtcNow - referenceDate.Value).TotalDays >= 7)
                {
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
                        
                        // Notify manager
                        await notificationService.SendAsync(
                            manager.Id, 
                            "Weekly Update Due", 
                            $"The 7-day scheduled update is now due for {booking.Vehicle?.RegistrationNo}. Please perform the condition check.", 
                            "WeeklyTask"
                        );
                    }
                }
            }

            await dbContext.SaveChangesAsync(stoppingToken);
        }
    }
}

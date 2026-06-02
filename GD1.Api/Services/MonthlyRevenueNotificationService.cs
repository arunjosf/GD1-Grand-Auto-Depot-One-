using GD1.Application.Interfaces.Services;
using GD1.Domain.Entities;
using GD1.Domain.Entities.Enums;
using GD1.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Api.Services
{
    public class MonthlyRevenueNotificationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MonthlyRevenueNotificationService> _logger;

        public MonthlyRevenueNotificationService(
            IServiceProvider serviceProvider,
            ILogger<MonthlyRevenueNotificationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;
                
                // Run on the 1st of every month at midnight
                if (now.Day == 1 && now.Hour == 0 && now.Minute == 0)
                {
                    await ProcessMonthlyRevenueAsync(now);
                    try { await Task.Delay(TimeSpan.FromHours(1), stoppingToken); }
                    catch (OperationCanceledException) { break; }
                }
                else
                {
                    // Check every 10 minutes
                    try { await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken); }
                    catch (OperationCanceledException) { break; }
                }
            }
        }

        private async Task ProcessMonthlyRevenueAsync(DateTime currentMonthDate)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var userRepo = scope.ServiceProvider.GetRequiredService<IGenericRepository<User>>();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                var bookingRepo = scope.ServiceProvider.GetRequiredService<IGenericRepository<Booking>>();

                var admins = await userRepo.FindAsync(u => u.Role == UserRole.GD1Admin);
                if (!admins.Any()) return;

                var lastMonthStart = new DateTime(currentMonthDate.Year, currentMonthDate.Month, 1).AddMonths(-1);
                var lastMonthEnd = lastMonthStart.AddMonths(1).AddTicks(-1);

                var bookings = await bookingRepo.FindAsync(b => b.CreatedAt >= lastMonthStart && b.CreatedAt <= lastMonthEnd);
                
                decimal totalRevenue = bookings.Sum(b => b.TotalCost);
                decimal platformFee = totalRevenue * 0.1m; // 10%

                foreach (var admin in admins)
                {
                    await notificationService.SendAsync(
                        userId: admin.Id,
                        title: "Monthly Revenue Report",
                        body: $"Platform revenue for {lastMonthStart:MMMM yyyy} was ${platformFee:F2}.",
                        actionType: "ViewRevenue",
                        referenceId: 0);
                }

                _logger.LogInformation($"Sent monthly revenue report for {lastMonthStart:MMMM yyyy}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process monthly revenue notifications.");
            }
        }
    }
}

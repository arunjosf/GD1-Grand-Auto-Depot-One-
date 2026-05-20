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

namespace GD1.Infrastructure.Services
{
    public class BookingCleanupService : BackgroundService
    {
        private readonly ILogger<BookingCleanupService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public BookingCleanupService(ILogger<BookingCleanupService> logger, IServiceProvider serviceProvider)
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
                    using var scope = _serviceProvider.CreateScope();
                    var bookingRepo = scope.ServiceProvider.GetRequiredService<IGenericRepository<Booking>>();

                    // Find all bookings awaiting agreement that are older than 15 minutes
                    var threshold = DateTime.UtcNow.AddMinutes(-15);
                    var allBookings = await bookingRepo.GetAllAsync();
                    
                    var staleBookings = allBookings
                        .Where(b => b.Status == BookingStatus.AwaitingAgreement && b.CreatedAt <= threshold)
                        .ToList();

                    if (staleBookings.Any())
                    {
                        foreach (var booking in staleBookings)
                        {
                            _logger.LogInformation("Deleting stale AwaitingAgreement booking ID {BookingId} created at {CreatedAt}", booking.Id, booking.CreatedAt);
                            await bookingRepo.DeleteAsync(booking);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing BookingCleanupService.");
                }

                // Run every 5 minutes
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}

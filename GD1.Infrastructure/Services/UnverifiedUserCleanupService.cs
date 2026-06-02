using GD1.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GD1.Infrastructure.Services
{
    
    public class UnverifiedUserCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<UnverifiedUserCleanupService> _logger;
        private static readonly TimeSpan Interval = TimeSpan.FromSeconds(60);
        private static readonly TimeSpan GracePeriod = TimeSpan.FromMinutes(10);

        public UnverifiedUserCleanupService(
            IServiceScopeFactory scopeFactory,
            ILogger<UnverifiedUserCleanupService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("UnverifiedUserCleanupService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(Interval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                try
                {
                    await PurgeExpiredUnverifiedUsersAsync(stoppingToken);
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while purging unverified users.");
                }
            }

            _logger.LogInformation("UnverifiedUserCleanupService stopped.");
        }

        private async Task PurgeExpiredUnverifiedUsersAsync(CancellationToken ct)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var cutoff = DateTime.UtcNow - GracePeriod;

            var expired = await db.Users
                .Where(u => !u.IsEmailVerified && u.CreatedAt < cutoff)
                .ToListAsync(ct);

            if (expired.Count == 0)
                return;

            db.Users.RemoveRange(expired);
            await db.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Deleted {Count} unverified user(s) who did not verify within {Minutes} minutes.",
                expired.Count, GracePeriod.TotalMinutes);
        }
    }
}

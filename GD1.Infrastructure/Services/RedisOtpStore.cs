using GD1.Application.Interfaces;
using StackExchange.Redis;

namespace GD1.Infrastructure.Services
{
    public class RedisOtpStore : IRedisOtpStore
    {
        private readonly StackExchange.Redis.IDatabase _db;
        public RedisOtpStore(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        public async Task StoreOtpAsync(string email, string otpHash, TimeSpan expiry)
            => await _db.StringSetAsync($"otp:{email}", otpHash, expiry);

        public async Task<string?> GetOtpAsync(string email)
        {
            var value = await _db.StringGetAsync($"otp:{email}");
            return value.HasValue ? value.ToString() : null;
        }

        public async Task DeleteOtpAsync(string email)
            => await _db.KeyDeleteAsync($"otp:{email}");
    }
}
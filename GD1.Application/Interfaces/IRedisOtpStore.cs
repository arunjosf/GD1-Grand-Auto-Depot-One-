using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Application.Interfaces
{
    public interface IRedisOtpStore
    {
        Task StoreOtpAsync(string email, string otpHash, TimeSpan expiry);
        Task<string?> GetOtpAsync(string email);
        Task DeleteOtpAsync(string email);
    }
}

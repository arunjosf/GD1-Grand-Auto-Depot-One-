using GD1.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Infrastructure.Services
{
    public class OtpService : IOtpService
    {
        public string GenerateOtp()
            => new Random().Next(100000, 999999).ToString();

        public string HashOtp(string otp)
            => BCrypt.Net.BCrypt.HashPassword(otp);

        public bool VerifyOtp(string otp, string hash)
            => BCrypt.Net.BCrypt.Verify(otp, hash);

        public DateTime GetExpiry()
            => DateTime.UtcNow.AddMinutes(10);
    }
}

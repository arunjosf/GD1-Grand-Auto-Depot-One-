using GD1.Application.Features.Auth.DTOs;
using GD1.Domain.Entities;
using System.Threading.Tasks;

namespace GD1.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> GoogleLoginAsync(GoogleLoginRequest request);
        Task<AuthResponse> RefreshTokenAsync(string refreshToken);
        Task RevokeTokenAsync(string refreshToken);

        Task<string> SendVerificationOtpAsync(string email);
        Task<AuthResponse> VerifyEmailOtpAsync(VerifyOtpRequest request);
        Task<AuthResponse> CreateAuthResponseAsync(User user);

        Task<string> SendPasswordResetOtpAsync(string email);
        Task ResetPasswordAsync(string email, string otp, string newPassword);
    }
}
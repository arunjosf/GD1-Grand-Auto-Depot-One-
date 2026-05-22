using GD1.Application.Features.Auth.DTOs;
using GD1.Application.Interfaces;
using GD1.Domain.Entities;
using GD1.Domain.Entities.Enums;
using GD1.Infrastructure.Data;
using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using Microsoft.Extensions.Logging;

namespace GD1.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        private readonly IEmailService _email;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            AppDbContext db,
            IConfiguration config,
            IEmailService email,
            ILogger<AuthService> logger)
        {
            _db = db;
            _config = config;
            _email = email;
            _logger = logger;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest req)
        {
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Email == req.Email.ToLower().Trim());

            if (user != null)
            {
                if (user.IsEmailVerified)
                    throw new InvalidOperationException("Email already registered. Please login or use a different email.");
                
                // If not verified, allow "re-registering" (updates info and resends OTP)
                user.FullName = req.FullName.Trim();
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password);
                user.PhoneNumber = null;
                user.CreatedAt = DateTime.UtcNow; // Reset cleanup timer
            }
            else
            {
                user = new User
                {
                    FullName = req.FullName.Trim(),
                    Email = req.Email.ToLower().Trim(),
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
                    Role = UserRole.VehicleOwner,
                    IsActive = true,
                    IsEmailVerified = false,
                    PhoneNumber = null
                };
                _db.Users.Add(user);
            }

            await _db.SaveChangesAsync();
            
            try
            {
                await SendOtpEmailAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Initial OTP email failed for {Email} during registration.", user.Email);
            }

            return new AuthResponse
            {
                AccessToken = string.Empty,
                RefreshToken = string.Empty,
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                IsEmailVerified = false
            };
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest req)
        {
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Email == req.Email.ToLower().Trim());

            if (user is null || user.PasswordHash is null)
                throw new UnauthorizedAccessException("Invalid email or password.");

            if (!BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid email or password.");

            if (!user.IsActive)
                throw new UnauthorizedAccessException("Account is deactivated.");

            if (!user.IsEmailVerified)
                throw new UnauthorizedAccessException("Email not verified. Please verify your email before logging in.");



            return await BuildResponseAsync(user);
        }

        public async Task<AuthResponse> GoogleLoginAsync(GoogleLoginRequest req)
        {
            _logger.LogInformation("Google Login attempt started.");
            string email = "", name = "", picture = "", googleId = "";

            try {
                // 1. Try to validate as ID Token (JWT)
                if (req.IdToken.Contains("."))
                {
                    _logger.LogInformation("Attempting to validate Google ID Token.");
                    var settings = new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = [_config["Google:ClientId"]]
                    };
                    var payload = await GoogleJsonWebSignature.ValidateAsync(req.IdToken, settings);
                    email = payload.Email;
                    name = payload.Name;
                    picture = payload.Picture;
                    googleId = payload.Subject;
                }
                else
                {
                    // 2. Fallback to Access Token verification
                    _logger.LogInformation("Attempting to validate Google Access Token via API.");
                    using var client = new HttpClient();
                    var response = await client.GetAsync($"https://www.googleapis.com/oauth2/v3/userinfo?access_token={req.IdToken}");
                    if (!response.IsSuccessStatusCode)
                    {
                        var err = await response.Content.ReadAsStringAsync();
                        _logger.LogWarning("Google userinfo API failed: {Error}", err);
                        throw new UnauthorizedAccessException("Invalid Google access token.");
                    }

                    var content = await response.Content.ReadAsStringAsync();
                    using var doc = System.Text.Json.JsonDocument.Parse(content);
                    var root = doc.RootElement;
                    
                    email = root.TryGetProperty("email", out var e) ? e.GetString() : "";
                    name = root.TryGetProperty("name", out var n) ? n.GetString() : "";
                    picture = root.TryGetProperty("picture", out var p) ? p.GetString() : "";
                    googleId = root.TryGetProperty("sub", out var s) ? s.GetString() : "";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Google verification failed.");
                throw new UnauthorizedAccessException($"Google verification failed: {ex.Message}");
            }

            if (string.IsNullOrEmpty(email))
                throw new UnauthorizedAccessException("Could not retrieve email from Google.");

            var user = await _db.Users.FirstOrDefaultAsync(
                u => u.GoogleId == googleId || u.Email == email.ToLower().Trim());

            bool isNewUser = false;

            if (user is null)
            {
                user = new User
                {
                    FullName = name,
                    Email = email.ToLower().Trim(),
                    GoogleId = googleId,
                    AvatarUrl = picture,
                    Role = UserRole.VehicleOwner,
                    IsActive = true,
                    IsEmailVerified = true  
                };

                _db.Users.Add(user);
                await _db.SaveChangesAsync();
                isNewUser = true;
            }
            else 
            {
                // Update existing user info if linked
                if (user.GoogleId is null)
                {
                    user.GoogleId = googleId;
                    user.IsEmailVerified = true;
                }
                user.AvatarUrl = picture;
                await _db.SaveChangesAsync();
            }

            if (!user.IsActive)
                throw new UnauthorizedAccessException("Account is deactivated.");

            if (isNewUser)
                await SendWelcomeEmailAsync(user);

            var res = await BuildResponseAsync(user);
            res.IsNewUser = isNewUser;
            return res;
        }

        public async Task<string> SendVerificationOtpAsync(string email)
        {
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Email == email.ToLower().Trim());

            if (user is null)
            {
                _logger.LogWarning("OTP Request Failed: User with email {Email} not found in database.", email);
                throw new KeyNotFoundException("User not found.");
            }

            if (user.IsEmailVerified)
                throw new InvalidOperationException("Email already verified.");

            try
            {
                await SendOtpEmailAsync(user);
                
                if (_config.GetValue<bool>("Email:UseDevMode"))
                    return "DEV MODE: OTP sent to your backend console (terminal).";
                    
                return "OTP sent to your email address.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OTP email sending failed for {Email}.", email);
                throw new Exception($"OTP generated but email sending failed: {ex.Message}. " +
                    "TIP: If you can't fix Gmail credentials, set 'Email:UseDevMode': true in appsettings.json to see OTPs in the console instead.");
            }

        }

        public async Task<AuthResponse> VerifyEmailOtpAsync(VerifyOtpRequest req)
        {
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Email == req.Email.ToLower().Trim());

            if (user is null)
                throw new KeyNotFoundException("User not found.");

            if (user.IsEmailVerified)
                throw new InvalidOperationException("Email already verified.");

            if (user.EmailOtp is null || user.EmailOtpExpiry is null)
                throw new InvalidOperationException(
                    "No OTP found. Request a new one.");

            if (user.EmailOtpExpiry < DateTime.UtcNow)
                throw new InvalidOperationException(
                    "OTP has expired. Request a new one.");

            var submittedOtp = (req.Otp ?? "").Trim();
            _logger.LogInformation("Attempting to verify OTP for {Email}. Length: {Length}", user.Email, submittedOtp.Length);

            if (!BCrypt.Net.BCrypt.Verify(submittedOtp, user.EmailOtp))
            {
                _logger.LogWarning("Invalid OTP attempt for {Email}.", user.Email);
                throw new UnauthorizedAccessException("Incorrect OTP.");
            }

            user.IsEmailVerified = true;
            user.EmailOtp = null;
            user.EmailOtpExpiry = null;

            await _db.SaveChangesAsync();

            await SendWelcomeEmailAsync(user);

            return await BuildResponseAsync(user);
        }

        public async Task<AuthResponse> CreateAuthResponseAsync(User user)
        {
            return await BuildResponseAsync(user);
        }

        public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
        {
            var stored = await _db.RefreshTokens
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Token == refreshToken);

            if (stored is null)
                throw new UnauthorizedAccessException("Invalid refresh token.");

            if (stored.IsRevoked)
                throw new UnauthorizedAccessException("Refresh token revoked.");

            if (stored.ExpiresAt < DateTime.UtcNow)
                throw new UnauthorizedAccessException("Refresh token expired.");

            if (!stored.User.IsActive)
                throw new UnauthorizedAccessException("Account is deactivated.");

            stored.IsRevoked = true;
            await _db.SaveChangesAsync();

            return await BuildResponseAsync(stored.User);
        }

        public async Task RevokeTokenAsync(string refreshToken)
        {
            var stored = await _db.RefreshTokens
                .FirstOrDefaultAsync(r => r.Token == refreshToken);

            if (stored is not null && !stored.IsRevoked)
            {
                stored.IsRevoked = true;
                await _db.SaveChangesAsync();
            }
        }

        public async Task<string> SendPasswordResetOtpAsync(string email)
        {
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Email == email.ToLower().Trim());

            // Always return a generic message to prevent user enumeration
            if (user is null)
            {
                _logger.LogWarning("Password reset OTP requested for unknown email {Email}.", email);
                return "If an account with this email exists, a reset OTP has been sent.";
            }

            if (!user.IsActive)
                throw new UnauthorizedAccessException("Account is deactivated.");

            try
            {
                var plainOtp = new Random().Next(100000, 999999).ToString();
                user.EmailOtp = BCrypt.Net.BCrypt.HashPassword(plainOtp);
                user.EmailOtpExpiry = DateTime.UtcNow.AddMinutes(10);
                await _db.SaveChangesAsync();

                var subject = "GD1 — Password Reset OTP";
                var body = $@"
<!DOCTYPE html>
<html>
<body style='font-family: Arial, sans-serif; background:#f5f5f5; padding:30px;'>
  <div style='max-width:480px; margin:auto; background:white; border-radius:8px;
              padding:40px; box-shadow:0 2px 8px rgba(0,0,0,0.1);'>
    <h2 style='color:#1a1a1a; margin-bottom:4px;'>GD1</h2>
    <p style='color:#666; font-size:13px; margin-top:0;'>Grand Auto Depot One</p>
    <hr style='border:none; border-top:1px solid #eee; margin:24px 0;'>
    <h3 style='color:#1a1a1a;'>Reset your password</h3>
    <p style='color:#444; font-size:14px;'>
      Hi {user.FullName}, use the OTP below to reset your GD1 account password.
      It is valid for <strong>10 minutes</strong>.
    </p>
    <div style='text-align:center; margin:32px 0;'>
      <div style='display:inline-block; background:#fff4f0; border:1px solid #ffc7b3;
                  border-radius:8px; padding:16px 40px;'>
        <span style='font-size:36px; font-weight:bold; letter-spacing:12px;
                     color:#e05a00;'>{plainOtp}</span>
      </div>
    </div>
    <p style='color:#999; font-size:12px; text-align:center;'>
      If you did not request a password reset, please ignore this email.
      Your password will remain unchanged.
    </p>
  </div>
</body>
</html>";

                await _email.SendAsync(user.Email, subject, body);

                if (_config.GetValue<bool>("Email:UseDevMode"))
                    return "DEV MODE: Password reset OTP sent to your backend console (terminal).";

                return "If an account with this email exists, a reset OTP has been sent.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Password reset OTP email sending failed for {Email}.", email);
                throw new Exception($"OTP generated but email sending failed: {ex.Message}.");
            }
        }

        public async Task ResetPasswordAsync(string email, string otp, string newPassword)
        {
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Email == email.ToLower().Trim());

            if (user is null)
                throw new KeyNotFoundException("User not found.");

            if (!user.IsActive)
                throw new UnauthorizedAccessException("Account is deactivated.");

            if (user.EmailOtp is null || user.EmailOtpExpiry is null)
                throw new InvalidOperationException("No password reset OTP found. Please request a new one.");

            if (user.EmailOtpExpiry < DateTime.UtcNow)
                throw new InvalidOperationException("OTP has expired. Please request a new one.");

            var submittedOtp = (otp ?? "").Trim();
            _logger.LogInformation("Password reset OTP verification attempt for {Email}.", user.Email);

            if (!BCrypt.Net.BCrypt.Verify(submittedOtp, user.EmailOtp))
            {
                _logger.LogWarning("Invalid password reset OTP for {Email}.", user.Email);
                throw new UnauthorizedAccessException("Incorrect OTP.");
            }

            // Update password and clear OTP
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.EmailOtp = null;
            user.EmailOtpExpiry = null;

            // Revoke all existing refresh tokens for security
            var tokens = await _db.RefreshTokens
                .Where(t => t.UserId == user.Id && !t.IsRevoked)
                .ToListAsync();
            foreach (var token in tokens)
                token.IsRevoked = true;

            await _db.SaveChangesAsync();

            _logger.LogInformation("Password reset successful for {Email}.", user.Email);
        }

        private async Task<AuthResponse> BuildResponseAsync(User user)
        {
            // Block Agents who are not yet approved
            if (user.Role == UserRole.Agent)
            {
                var agent = await _db.Agents.FirstOrDefaultAsync(a => a.Id == user.Id);
                if (agent == null || agent.ApprovalStatus != AgentApprovalStatus.Approved)
                    throw new UnauthorizedAccessException("Your agent account is pending Admin approval. You will be able to login once approved.");
            }

            // Block Managers who are not yet approved by their LotOwner
            if (user.Role == UserRole.Manager)
            {
                var hasApproved = await _db.LotManagers
                    .AnyAsync(m => m.ManagerId == user.Id && m.ApprovalStatus == AgentApprovalStatus.Approved);
                if (!hasApproved)
                    throw new UnauthorizedAccessException("Your manager account is pending approval from your lot owner.");
            }

            var accessToken = GenerateAccessToken(user);
            var refreshToken = await SaveRefreshTokenAsync(user.Id);

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                IsEmailVerified = user.IsEmailVerified
            };
        }

        private string GenerateAccessToken(User user)
        {
            var secretKey = _config["Jwt:SecretKey"]
                ?? throw new InvalidOperationException("Jwt:SecretKey not configured.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim("userId", user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("role", user.Role.ToString()),
                new Claim("roleId", ((int)user.Role).ToString()),
                new Claim("fullName", user.FullName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var expiryMinutes = int.Parse(
                _config["Jwt:AccessTokenExpiryMinutes"] ?? "15");

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<string> SaveRefreshTokenAsync(long userId)
        {
            var previous = await _db.RefreshTokens
                .Where(r => r.UserId == userId)
                .ToListAsync();

            _db.RefreshTokens.RemoveRange(previous);

            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            var expiryDays = int.Parse(
                _config["Jwt:RefreshTokenExpiryDays"] ?? "7");

            _db.RefreshTokens.Add(new RefreshToken
            {
                UserId = userId,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
                IsRevoked = false
            });

            await _db.SaveChangesAsync();
            return token;
        }

        private async Task SendOtpEmailAsync(User user)
        {
            var plainOtp = new Random().Next(100000, 999999).ToString();

            user.EmailOtp = BCrypt.Net.BCrypt.HashPassword(plainOtp);
            user.EmailOtpExpiry = DateTime.UtcNow.AddMinutes(10);
            await _db.SaveChangesAsync();

            var subject = "GD1 — Verify Your Email";
            var body = $@"
<!DOCTYPE html>
<html>
<body style='font-family: Arial, sans-serif; background:#f5f5f5; padding:30px;'>
  <div style='max-width:480px; margin:auto; background:white; border-radius:8px;
              padding:40px; box-shadow:0 2px 8px rgba(0,0,0,0.1);'>
    <h2 style='color:#1a1a1a; margin-bottom:4px;'>GD1</h2>
    <p style='color:#666; font-size:13px; margin-top:0;'>Grand Auto Depot One</p>
    <hr style='border:none; border-top:1px solid #eee; margin:24px 0;'>
    <h3 style='color:#1a1a1a;'>Verify your email address</h3>
    <p style='color:#444; font-size:14px;'>
      Hi {user.FullName}, use this OTP to verify your email address.
      It is valid for <strong>10 minutes</strong>.
    </p>
    <div style='text-align:center; margin:32px 0;'>
      <div style='display:inline-block; background:#f0f4ff; border:1px solid #c7d7ff;
                  border-radius:8px; padding:16px 40px;'>
        <span style='font-size:36px; font-weight:bold; letter-spacing:12px;
                     color:#2563eb;'>{plainOtp}</span>
      </div>
    </div>
    <p style='color:#999; font-size:12px; text-align:center;'>
      If you did not create a GD1 account, ignore this email.
    </p>
  </div>
</body>
</html>";

            await _email.SendAsync(user.Email, subject, body);
        }

        private async Task SendWelcomeEmailAsync(User user)
        {
            var frontendUrl = _config["Frontend:BaseUrl"] ?? "http://localhost:5173";
            var subject = "Welcome to GD1 — Your Vehicle Is in Safe Hands";
            var body = $@"
<!DOCTYPE html>
<html>
<body style='font-family: Arial, sans-serif; background:#f5f5f5; padding:30px;'>
  <div style='max-width:480px; margin:auto; background:white; border-radius:8px;
              padding:40px; box-shadow:0 2px 8px rgba(0,0,0,0.1);'>
    <h2 style='color:#1a1a1a; margin-bottom:4px;'>GD1</h2>
    <p style='color:#666; font-size:13px; margin-top:0;'>Grand Auto Depot One</p>
    <hr style='border:none; border-top:1px solid #eee; margin:24px 0;'>
    <h3 style='color:#1a1a1a;'>Welcome, {user.FullName}!</h3>
    <p style='color:#444; font-size:14px; line-height:1.7;'>
      Your GD1 account is ready. We provide secure long-term vehicle storage
      with live monitoring, scheduled maintenance, and complete peace of mind —
      no matter where life takes you.
    </p>
    <div style='margin:32px 0; text-align:center;'>
      <a href='{frontendUrl}/dashboard'
         style='background:#2563eb; color:white; text-decoration:none;
                padding:12px 32px; border-radius:4px; font-size:14px;
                font-weight:600; display:inline-block;'>
        Go to Dashboard
      </a>
    </div>
    <p style='color:#999; font-size:12px;'>
      GD1 · Grand Auto Depot One · Where every machine rests in safety.
    </p>
  </div>
</body>
</html>";

            await _email.SendAsync(user.Email, subject, body);
        }
    }
}


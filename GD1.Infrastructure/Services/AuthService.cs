using GD1.Application.Features.Auth.DTOs;
using GD1.Application.Interfaces;
using GD1.Domain.Entities;
using GD1.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using GD1.Domain.Entities.Enums;

namespace GD1.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;

        public AuthService(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest req)
        {
            var emailExists = await _db.Users
                .AnyAsync(u => u.Email == req.Email.ToLower().Trim());

            if (emailExists)
                throw new InvalidOperationException("Email already registered.");

            var user = new User
            {
                FullName = req.FullName.Trim(),
                Email = req.Email.ToLower().Trim(),
                PhoneNumber = req.PhoneNumber,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
                Role = UserRole.VehicleOwner,
                IsActive = true,
                IsEmailVerified = false
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return await BuildResponseAsync(user);
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

            return await BuildResponseAsync(user);
        }

        public async Task<AuthResponse> GoogleLoginAsync(GoogleLoginRequest req)
        {
            GoogleJsonWebSignature.Payload payload;

            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(req.IdToken);
            }
            catch
            {
                throw new UnauthorizedAccessException("Invalid Google token.");
            }

            var user = await _db.Users.FirstOrDefaultAsync(
                u => u.GoogleId == payload.Subject || u.Email == payload.Email);

            if (user is null)
            {
                user = new User
                {
                    FullName = payload.Name,
                    Email = payload.Email.ToLower().Trim(),
                    GoogleId = payload.Subject,
                    AvatarUrl = payload.Picture,
                    Role = UserRole.VehicleOwner,
                    IsActive = true,
                    IsEmailVerified = true
                };

                _db.Users.Add(user);
                await _db.SaveChangesAsync();
            }
            else if (user.GoogleId is null)
            {
                user.GoogleId = payload.Subject;
                user.AvatarUrl = payload.Picture;
                await _db.SaveChangesAsync();
            }

            if (!user.IsActive)
                throw new UnauthorizedAccessException("Account is deactivated.");

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
                throw new UnauthorizedAccessException("Refresh token has been revoked.");

            if (stored.ExpiresAt < DateTime.UtcNow)
                throw new UnauthorizedAccessException("Refresh token has expired.");

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

        private async Task<AuthResponse> BuildResponseAsync(User user)
        {
            var accessToken = GenerateAccessToken(user);
            var refreshToken = await SaveRefreshTokenAsync(user.Id);

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role
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
                new Claim(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
                new Claim("userId",   user.Id.ToString(), ClaimValueTypes.Integer64),
                new Claim("fullName", user.FullName),
                new Claim("role",     ((int)user.Role).ToString(), ClaimValueTypes.Integer32)
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
                .Where(r => r.UserId == userId && !r.IsRevoked)
                .ToListAsync();

            foreach (var t in previous)
                t.IsRevoked = true;

            var tokenBytes = RandomNumberGenerator.GetBytes(64);
            var token = Convert.ToBase64String(tokenBytes);

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
    }
}
    

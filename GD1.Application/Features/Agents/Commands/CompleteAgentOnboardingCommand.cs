using GD1.Application.Common;
using GD1.Application.Features.Auth.DTOs;
using GD1.Application.Interfaces;
using GD1.Domain.Entities;
using GD1.Domain.Interfaces;
using MediatR;
using FluentValidation;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

namespace GD1.Application.Features.Agents.Commands
{
    public class CompleteAgentOnboardingCommand : IRequest<BaseResponse<AuthResponse>>
    {
        public string Email { get; set; } = string.Empty;
        public string Otp { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
    }

    public class CompleteAgentOnboardingCommandValidator : AbstractValidator<CompleteAgentOnboardingCommand>
    {
        public CompleteAgentOnboardingCommandValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Otp).NotEmpty().Length(6);
            RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
            RuleFor(x => x.ConfirmPassword).Equal(x => x.Password).WithMessage("Passwords do not match.");
        }
    }

    public class CompleteAgentOnboardingCommandHandler 
        : IRequestHandler<CompleteAgentOnboardingCommand, BaseResponse<AuthResponse>>
    {
        private readonly IAuthService _auth;
        private readonly IGenericRepository<User> _userRepo;

        public CompleteAgentOnboardingCommandHandler(IAuthService auth, IGenericRepository<User> userRepo)
        {
            _auth = auth;
            _userRepo = userRepo;
        }

        public async Task<BaseResponse<AuthResponse>> Handle(CompleteAgentOnboardingCommand cmd, CancellationToken ct)
        {
            var verifyReq = new VerifyOtpRequest 
            { 
                Email = cmd.Email, 
                Otp = cmd.Otp 
            };
            
            var authResponse = await _auth.VerifyEmailOtpAsync(verifyReq);

            var user = (await _userRepo.FindAsync(u => u.Email == cmd.Email.ToLower().Trim())).FirstOrDefault();
            if (user != null)
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(cmd.Password);
                
                if (!string.IsNullOrEmpty(cmd.PhoneNumber))
                {
                    user.PhoneNumber = cmd.PhoneNumber;
                }

                user.IsActive = true;
                user.IsEmailVerified = true;
                
                await _userRepo.UpdateAsync(user);

                return BaseResponse<AuthResponse>.Ok(authResponse, "Onboarding completed successfully. Your account is now pending Admin approval.");
            }

            return BaseResponse<AuthResponse>.Fail("User not found.");
        }
    }
}

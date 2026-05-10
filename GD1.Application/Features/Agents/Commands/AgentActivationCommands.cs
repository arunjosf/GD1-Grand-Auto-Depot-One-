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
using System;
using System.ComponentModel.DataAnnotations;

namespace GD1.Application.Features.Agents.Commands
{
    public class FinalizeAgentOnboardingCommand : IRequest<BaseResponse<AuthResponse>>
    {

        public string Token { get; set; } = string.Empty;

        [Required]
        [Phone(ErrorMessage ="Enter a valid phone number")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        [RegularExpression(
       @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$",
       ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character.")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required.")]
        [Compare("Password", ErrorMessage = "Passwords do not match. Please ensure password and confirm password are the same.")]
        public string? ConfirmPassword { get; set; }
    }

    public class FinalizeAgentOnboardingCommandHandler : IRequestHandler<FinalizeAgentOnboardingCommand, BaseResponse<AuthResponse>>
    {
        private readonly IGenericRepository<User> _userRepo;
        private readonly IGenericRepository<Agent> _agentRepo;
        private readonly IAuthService _auth;

        public FinalizeAgentOnboardingCommandHandler(IGenericRepository<User> userRepo, IGenericRepository<Agent> agentRepo, IAuthService auth)
        {
            _userRepo = userRepo;
            _agentRepo = agentRepo;
            _auth = auth;
        }

        public async Task<BaseResponse<AuthResponse>> Handle(FinalizeAgentOnboardingCommand cmd, CancellationToken ct)
        {
            var agent = (await _agentRepo.FindAsync(a => a.InvitationToken == cmd.Token)).FirstOrDefault();
            if (agent == null) return BaseResponse<AuthResponse>.Fail("Invalid or expired invitation token.");

            var user = await _userRepo.GetByIdAsync(agent.UserId);
            if (user == null) return BaseResponse<AuthResponse>.Fail("User record not found.");

            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                if (string.IsNullOrEmpty(cmd.Password) || cmd.Password != cmd.ConfirmPassword)
                    return BaseResponse<AuthResponse>.Fail("A valid password and confirmation are required for new accounts.");
                
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(cmd.Password);
            }

            if (string.IsNullOrEmpty(user.PhoneNumber))
            {
                if (string.IsNullOrEmpty(cmd.PhoneNumber))
                    return BaseResponse<AuthResponse>.Fail("Mobile number is required to activate your agent account.");
                
                user.PhoneNumber = cmd.PhoneNumber;
            }

            user.IsActive = true;
            user.IsEmailVerified = true; 
            await _userRepo.UpdateAsync(user);

            agent.PhoneNumber = user.PhoneNumber ?? string.Empty;
            agent.IsVerified = true;
            agent.IsActive = true;
            agent.InvitationToken = null;
            await _agentRepo.UpdateAsync(agent);

            return BaseResponse<AuthResponse>.Ok(null, "Your profile has been submitted successfully. Please wait for Admin approval before logging in.");
        }
    }

    public class AddUserMobileCommand : IRequest<BaseResponse<bool>>
    {
        public long UserId { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
    }

    public class AddUserMobileCommandHandler : IRequestHandler<AddUserMobileCommand, BaseResponse<bool>>
    {
        private readonly IGenericRepository<User> _userRepo;

        public AddUserMobileCommandHandler(IGenericRepository<User> userRepo)
        {
            _userRepo = userRepo;
        }

        public async Task<BaseResponse<bool>> Handle(AddUserMobileCommand cmd, CancellationToken ct)
        {
            var user = await _userRepo.GetByIdAsync(cmd.UserId);
            if (user == null) return BaseResponse<bool>.Fail("User not found.");

            if (!string.IsNullOrEmpty(user.PhoneNumber))
                return BaseResponse<bool>.Fail("Mobile number is already registered for this account.");

            user.PhoneNumber = cmd.PhoneNumber;
            await _userRepo.UpdateAsync(user);

            return BaseResponse<bool>.Ok(true, "Mobile number added successfully.");
        }
    }
}

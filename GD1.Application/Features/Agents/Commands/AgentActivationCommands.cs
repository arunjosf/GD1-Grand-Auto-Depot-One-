using GD1.Application.Common;
using GD1.Application.Features.Auth.DTOs;
using GD1.Application.Interfaces;
using GD1.Domain.Entities;
using GD1.Domain.Entities.Enums;
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
    public class FinalizeOnboardingCommand : IRequest<BaseResponse<AuthResponse>>
    {

        public string Token { get; set; } = string.Empty;

        [Required]
        [Phone(ErrorMessage ="Enter a valid phone number")]
        public string? PhoneNumber { get; set; }

        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        [RegularExpression(
       @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$",
       ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character.")]
        public string? Password { get; set; }

        [Compare("Password", ErrorMessage = "Passwords do not match. Please ensure password and confirm password are the same.")]
        public string? ConfirmPassword { get; set; }
    }

    public class FinalizeOnboardingCommandHandler : IRequestHandler<FinalizeOnboardingCommand, BaseResponse<AuthResponse>>
    {
        private readonly IGenericRepository<User> _userRepo;
        private readonly IGenericRepository<Agent> _agentRepo;
        private readonly IAuthService _auth;

        public FinalizeOnboardingCommandHandler(IGenericRepository<User> userRepo, IGenericRepository<Agent> agentRepo, IAuthService auth)
        {
            _userRepo = userRepo;
            _agentRepo = agentRepo;
            _auth = auth;
        }

        public async Task<BaseResponse<AuthResponse>> Handle(FinalizeOnboardingCommand cmd, CancellationToken ct)
        {
            // --- Try Agent path first (token on Agent entity) ---
            var agent = (await _agentRepo.FindAsync(a => a.InvitationToken == cmd.Token)).FirstOrDefault();
            if (agent != null)
            {
                var user = await _userRepo.GetByIdAsync(agent.Id);
                if (user == null) return BaseResponse<AuthResponse>.Fail("User record not found.");

                if (string.IsNullOrEmpty(user.PasswordHash))
                {
                    if (string.IsNullOrEmpty(cmd.Password) || cmd.Password != cmd.ConfirmPassword)
                        return BaseResponse<AuthResponse>.Fail("A valid password and confirmation are required.");
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

                agent.IsActive = true;
                agent.InvitationToken = null;
                agent.ApprovalStatus = AgentApprovalStatus.Pending;
                await _agentRepo.UpdateAsync(agent);

                return BaseResponse<AuthResponse>.Ok(null, "Your profile has been submitted. Please wait for Admin approval before logging in.");
            }

            // --- Manager path (token on User entity) ---
            var managerUsers = await _userRepo.FindAsync(u => u.InvitationToken == cmd.Token);
            var managerUser = managerUsers.FirstOrDefault();

            if (managerUser == null)
                return BaseResponse<AuthResponse>.Fail("Invalid or expired invitation token.");

            if (managerUser.Role != UserRole.Manager)
                return BaseResponse<AuthResponse>.Fail("This invitation link is not valid for your account type.");

            if (string.IsNullOrEmpty(managerUser.PasswordHash))
            {
                if (string.IsNullOrEmpty(cmd.Password) || cmd.Password != cmd.ConfirmPassword)
                    return BaseResponse<AuthResponse>.Fail("A valid password and confirmation are required.");

                managerUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(cmd.Password);
            }

            managerUser.IsEmailVerified = true;
            managerUser.InvitationToken = null;

            if (!string.IsNullOrWhiteSpace(cmd.PhoneNumber))
                managerUser.PhoneNumber = cmd.PhoneNumber;
            else if (string.IsNullOrWhiteSpace(managerUser.PhoneNumber))
                return BaseResponse<AuthResponse>.Fail("Mobile number is required to activate your manager account.");

            // Manager stays inactive until LotOwner approves
            managerUser.IsActive = false;
            await _userRepo.UpdateAsync(managerUser);

            return BaseResponse<AuthResponse>.Ok(null, "Onboarding complete. Your access is pending approval from your lot owner.");
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

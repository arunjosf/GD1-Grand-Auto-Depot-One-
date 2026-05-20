using FluentValidation;
using GD1.Application.Common;
using GD1.Application.Interfaces;
using GD1.Domain.Entities;
using GD1.Domain.Entities.Enums;
using GD1.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Application.Features.Agents.Commands
{
    public class InviteOrUpgradeAgentCommand : IRequest<BaseResponse<long>>
    {
        [Required]
        [RegularExpression(@"^[A-Za-z][A-Za-z\s]*$",
            ErrorMessage = "Agent name must contain only letters and spaces.")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Url(ErrorMessage = "Invalid Agent Selfie URL.")]
        public string? SelfieUrl { get; set; }

        [Required]
        [Url(ErrorMessage = "Invalid Agent Id Image URL.")]
        public string? IdProofUrl { get; set; }


        [Required]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "City must contain only letters.")]
        public string? City { get; set; }


        [Required]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "State must contain only letters.")]
        public string? State { get; set; }

        [Required]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Postal Code must be exactly 6 digits.")]
        public string? PostalCode { get; set; }
    }

    public class InviteOrUpgradeAgentCommandValidator : AbstractValidator<InviteOrUpgradeAgentCommand>
    {
       
    }

    public class InviteOrUpgradeAgentCommandHandler : IRequestHandler<InviteOrUpgradeAgentCommand, BaseResponse<long>>
    {
        private readonly IGenericRepository<User> _userRepo;
        private readonly IGenericRepository<Agent> _agentRepo;
        private readonly IEmailService _email;
        private readonly IConfiguration _config;
        private readonly IGeocodingService _geocoding;

        public InviteOrUpgradeAgentCommandHandler(
            IGenericRepository<User> userRepo,
            IGenericRepository<Agent> agentRepo,
            IEmailService email,
            IConfiguration config,
            IGeocodingService geocoding)
        {
            _userRepo = userRepo;
            _agentRepo = agentRepo;
            _email = email;
            _config = config;
            _geocoding = geocoding;
        }

        public async Task<BaseResponse<long>> Handle(InviteOrUpgradeAgentCommand cmd, CancellationToken cancellationToken)
        {
            var email = cmd.Email.ToLower().Trim();
            var existingUser = (await _userRepo.FindAsync(u => u.Email == email)).FirstOrDefault();

            double? lat = null;
            double? lon = null;
            if (!string.IsNullOrEmpty(cmd.City))
            {
                var searchStr = $"{cmd.City}, {cmd.State ?? ""}, {cmd.PostalCode ?? ""}, India";
                var coords = await _geocoding.GetCoordinatesAsync(searchStr);
                if (coords.HasValue)
                {
                    lat = coords.Value.Lat;
                    lon = coords.Value.Lon;
                }
            }

            if (existingUser != null)
            {
                if (existingUser.Role == UserRole.GD1Admin)
                    return BaseResponse<long>.Fail("An Admin cannot be changed to an Agent role.");

                if (existingUser.Role != UserRole.Agent)
                {
                    existingUser.Role = UserRole.Agent;
                    await _userRepo.UpdateAsync(existingUser);
                }

                var agentProfile = (await _agentRepo.FindAsync(a => a.Id == existingUser.Id)).FirstOrDefault();
                var token = Guid.NewGuid().ToString("N");

                if (agentProfile == null)
                {
                    agentProfile = new Agent
                    {
                        Id = existingUser.Id,
                        City = cmd.City ?? "Unknown",
                        State = cmd.State ?? "Unknown",
                        PostalCode = cmd.PostalCode,
                        SelfieUrl = cmd.SelfieUrl,
                        IdProofUrl = cmd.IdProofUrl,
                        Latitude = lat,
                        Longitude = lon,
                        IsActive = true,
                        IsVerified = false,
                        InvitationToken = token
                    };
                    await _agentRepo.AddAsync(agentProfile);
                }
                else
                {
                    agentProfile.InvitationToken = token;
                    agentProfile.SelfieUrl = cmd.SelfieUrl;
                    agentProfile.IdProofUrl = cmd.IdProofUrl;
                    agentProfile.Latitude = lat;
                    agentProfile.Longitude = lon;
                    await _agentRepo.UpdateAsync(agentProfile);
                }

                await SendInvitationEmail(existingUser, token);
                return BaseResponse<long>.Ok(existingUser.Id, "User has been appointed as Agent. Invitation email sent.");
            }
            else
            {
                var token = Guid.NewGuid().ToString("N");
                var newUser = new User
                {
                    FullName = cmd.FullName,
                    Email = email,
                    PasswordHash = null, 
                    Role = UserRole.Agent,
                    IsActive = true,
                    IsEmailVerified = false
                };
                await _userRepo.AddAsync(newUser);

                var newAgent = new Agent
                {
                    Id = newUser.Id,
                    City = cmd.City ?? "Unknown",
                    State = cmd.State ?? "Unknown",
                    PostalCode = cmd.PostalCode,
                    SelfieUrl = cmd.SelfieUrl,
                    IdProofUrl = cmd.IdProofUrl,
                    Latitude = lat,
                    Longitude = lon,
                    IsActive = true,
                    IsVerified = false,
                    InvitationToken = token
                };
                await _agentRepo.AddAsync(newAgent);

                await SendInvitationEmail(newUser, token);
                return BaseResponse<long>.Ok(newUser.Id, "Agent invited. They will receive an email to complete their profile.");
            }
        }

        private async Task SendInvitationEmail(User user, string token)
        {
            var frontendUrl = _config["Frontend:BaseUrl"] ?? "http://localhost:5173";
            var subject = "Welcome to the GD1 Agent Team!";
            var body = $@"
<!DOCTYPE html>
<html>
<body style='font-family: Arial, sans-serif; background:#f5f5f5; padding:30px;'>
  <div style='max-width:480px; margin:auto; background:white; border-radius:8px; padding:40px; box-shadow:0 2px 8px rgba(0,0,0,0.1);'>
    <h2 style='color:#1a1a1a;'>Welcome to GD1, {user.FullName}!</h2>
    <p style='color:#444; font-size:14px; line-height:1.6;'>
      You have been invited to join the GD1 professional agent network. 
      To get started and activate your account, please click the button below to complete your profile.
    </p>
    <div style='text-align:center; margin:35px 0;'>
      <a href='{frontendUrl}/agent/onboarding?token={token}' 
         style='background:#2563eb; color:white; padding:14px 32px; text-decoration:none; border-radius:6px; font-weight:600; display:inline-block;'>
         Complete Your Profile
      </a>
    </div>
    <p style='color:#999; font-size:12px;'>
      If you did not expect this invitation, please ignore this email.
    </p>
  </div>
</body>
</html>";
            await _email.SendAsync(user.Email, subject, body);
        }
    }
}

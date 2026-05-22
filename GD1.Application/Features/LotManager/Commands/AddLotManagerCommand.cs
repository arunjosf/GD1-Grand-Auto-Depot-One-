using GD1.Application.Common;
using GD1.Application.Interfaces;
using GD1.Domain.Entities;
using GD1.Domain.Entities.Enums;
using GD1.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.LotManagement.Commands
{
    public class AddLotManagerDto
    {
        public long LotManagerRecordId { get; set; }
        public long ManagerUserId { get; set; }
        public string ManagerName { get; set; } = string.Empty;
        public string ManagerEmail { get; set; } = string.Empty;
        public long PropertyId { get; set; }
        public string PropertyName { get; set; } = string.Empty;
        public bool IsNewlyInvited { get; set; }
    }

    public class AddLotManagerCommand : IRequest<BaseResponse<AddLotManagerDto>>
    {
        public long LotOwnerId { get; set; }
        public long PropertyId { get; set; }
        public string ManagerEmail { get; set; } = string.Empty;
        public string? ManagerFullName { get; set; }
        public string? IdProofUrl { get; set; }
        public string? SelfieUrl { get; set; }
    }

    public class AddLotManagerCommandHandler : IRequestHandler<AddLotManagerCommand, BaseResponse<AddLotManagerDto>>
    {
        private readonly IGenericRepository<VehicleStorageProperty> _propertyRepo;
        private readonly IGenericRepository<User> _userRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.LotManager> _lotManagerRepo;
        private readonly IEmailService _email;
        private readonly IConfiguration _config;

        public AddLotManagerCommandHandler(
            IGenericRepository<VehicleStorageProperty> propertyRepo,
            IGenericRepository<User> userRepo,
            IGenericRepository<GD1.Domain.Entities.LotManager> lotManagerRepo,
            IEmailService email,
            IConfiguration config)
        {
            _propertyRepo = propertyRepo;
            _userRepo = userRepo;
            _lotManagerRepo = lotManagerRepo;
            _email = email;
            _config = config;
        }

        public async Task<BaseResponse<AddLotManagerDto>> Handle(AddLotManagerCommand cmd, CancellationToken ct)
        {
            // 1. Verify property belongs to this owner
            var property = await _propertyRepo.GetByIdAsync(cmd.PropertyId);
            if (property is null)
                return BaseResponse<AddLotManagerDto>.Fail("Property not found.");
            if (property.LotOwnerId != cmd.LotOwnerId)
                return BaseResponse<AddLotManagerDto>.Fail("You do not own this property.");

            var normalizedEmail = cmd.ManagerEmail.ToLower().Trim();
            var users = await _userRepo.FindAsync(u => u.Email == normalizedEmail);
            var managerUser = users.FirstOrDefault();
            bool isNewlyInvited = false;

            if (managerUser is null)
            {
                // --- INVITE PATH ---
                if (string.IsNullOrWhiteSpace(cmd.ManagerFullName))
                    return BaseResponse<AddLotManagerDto>.Fail(
                        "This email is not registered. Provide 'ManagerFullName' to invite them.");

                var invitationToken = Guid.NewGuid().ToString("N");

                managerUser = new User
                {
                    FullName = cmd.ManagerFullName.Trim(),
                    Email = normalizedEmail,
                    PasswordHash = null,
                    Role = UserRole.Manager,
                    IsActive = false,           // inactive until onboarding complete
                    IsEmailVerified = false,
                    InvitationToken = invitationToken
                };
                await _userRepo.AddAsync(managerUser);
                isNewlyInvited = true;

                await SendInviteEmailAsync(managerUser, property.Name, invitationToken);
            }
            else
            {
                // --- EXISTING USER PATH ---
                if (managerUser.Id == cmd.LotOwnerId)
                    return BaseResponse<AddLotManagerDto>.Fail("You cannot add yourself as a manager.");
                if (managerUser.Role == UserRole.GD1Admin)
                    return BaseResponse<AddLotManagerDto>.Fail("A GD1 Admin cannot be assigned as a manager.");

                var existingManagers = await _lotManagerRepo.FindAsync(
                    m => m.PropertyId == cmd.PropertyId && m.ManagerId == managerUser.Id);
                if (existingManagers.Any(m => m.IsActive))
                    return BaseResponse<AddLotManagerDto>.Fail("This user is already an active manager for this property.");

                if (managerUser.Role == UserRole.VehicleOwner)
                {
                    managerUser.Role = UserRole.Manager;
                    // Generate token so they go through the same finalize flow
                    managerUser.InvitationToken = Guid.NewGuid().ToString("N");
                    await _userRepo.UpdateAsync(managerUser);
                }

                await SendAppointmentEmailAsync(managerUser, property.Name, managerUser.InvitationToken!);
            }

            // 2. Create LotManager record — starts Pending, LotOwner must approve after onboarding
            var lotManager = new GD1.Domain.Entities.LotManager
            {
                PropertyId = cmd.PropertyId,
                ManagerId = managerUser.Id,
                AddedBy = cmd.LotOwnerId,
                IsActive = false,
                ApprovalStatus = AgentApprovalStatus.Pending,
                IdProofUrl = cmd.IdProofUrl,
                SelfieUrl = cmd.SelfieUrl
            };
            await _lotManagerRepo.AddAsync(lotManager);

            var msg = isNewlyInvited
                ? $"{managerUser.FullName} has been invited as a manager for '{property.Name}'. They will receive an email to complete onboarding."
                : $"{managerUser.FullName} has been added as a manager for '{property.Name}'. An onboarding email has been sent.";

            return BaseResponse<AddLotManagerDto>.Ok(new AddLotManagerDto
            {
                LotManagerRecordId = lotManager.Id,
                ManagerUserId = managerUser.Id,
                ManagerName = managerUser.FullName,
                ManagerEmail = managerUser.Email,
                PropertyId = property.Id,
                PropertyName = property.Name,
                IsNewlyInvited = isNewlyInvited
            }, msg);
        }

        private async Task SendInviteEmailAsync(User manager, string propertyName, string token)
        {
            var frontendUrl = _config["Frontend:BaseUrl"] ?? "http://localhost:5173";
            var subject = $"You've been invited as a Manager — {propertyName} | GD1";
            var body = $@"
<!DOCTYPE html>
<html>
<body style='font-family: Arial, sans-serif; background:#f5f5f5; padding:30px;'>
  <div style='max-width:480px; margin:auto; background:white; border-radius:8px;
              padding:40px; box-shadow:0 2px 8px rgba(0,0,0,0.1);'>
    <h2 style='color:#1a1a1a; margin-bottom:4px;'>GD1</h2>
    <p style='color:#666; font-size:13px; margin-top:0;'>Grand Auto Depot One</p>
    <hr style='border:none; border-top:1px solid #eee; margin:24px 0;'>
    <h3 style='color:#1a1a1a;'>Welcome, {manager.FullName}!</h3>
    <p style='color:#444; font-size:14px; line-height:1.7;'>
      You have been invited to be a <strong>Lot Manager</strong> for
      <strong>{propertyName}</strong> on the GD1 platform.
    </p>
    <p style='color:#444; font-size:14px;'>
      Click the button below to complete your profile and set your password.
      Your access will be active after the lot owner approves your request.
    </p>
    <div style='text-align:center; margin:32px 0;'>
      <a href='{frontendUrl}/onboarding?token={token}'
         style='background:#2563eb; color:white; text-decoration:none;
                padding:12px 32px; border-radius:4px; font-size:14px;
                font-weight:600; display:inline-block;'>
        Complete Onboarding
      </a>
    </div>
    <p style='color:#999; font-size:12px; text-align:center;'>
      If you did not expect this invitation, please ignore this email.
    </p>
  </div>
</body>
</html>";
            await _email.SendAsync(manager.Email, subject, body);
        }

        private async Task SendAppointmentEmailAsync(User manager, string propertyName, string token)
        {
            var frontendUrl = _config["Frontend:BaseUrl"] ?? "http://localhost:5173";
            var subject = $"You've been appointed as Manager — {propertyName} | GD1";
            var body = $@"
<!DOCTYPE html>
<html>
<body style='font-family: Arial, sans-serif; background:#f5f5f5; padding:30px;'>
  <div style='max-width:480px; margin:auto; background:white; border-radius:8px;
              padding:40px; box-shadow:0 2px 8px rgba(0,0,0,0.1);'>
    <h2 style='color:#1a1a1a; margin-bottom:4px;'>GD1</h2>
    <p style='color:#666; font-size:13px; margin-top:0;'>Grand Auto Depot One</p>
    <hr style='border:none; border-top:1px solid #eee; margin:24px 0;'>
    <h3 style='color:#1a1a1a;'>Hello, {manager.FullName}!</h3>
    <p style='color:#444; font-size:14px; line-height:1.7;'>
      You have been appointed as a <strong>Lot Manager</strong> for
      <strong>{propertyName}</strong> on GD1.
    </p>
    <p style='color:#444; font-size:14px;'>
      Please complete your onboarding to activate your manager access.
    </p>
    <div style='text-align:center; margin:32px 0;'>
      <a href='{frontendUrl}/onboarding?token={token}'
         style='background:#2563eb; color:white; text-decoration:none;
                padding:12px 32px; border-radius:4px; font-size:14px;
                font-weight:600; display:inline-block;'>
        Complete Onboarding
      </a>
    </div>
  </div>
</body>
</html>";
            await _email.SendAsync(manager.Email, subject, body);
        }
    }
}

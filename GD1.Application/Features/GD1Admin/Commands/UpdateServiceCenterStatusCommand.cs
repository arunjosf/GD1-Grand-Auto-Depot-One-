using GD1.Application.Common;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.GD1Admin.Commands
{
    public class UpdateServiceCenterStatusCommand : IRequest<BaseResponse<long>>
    {
        public long Id { get; set; }
        public GD1.Domain.Entities.Enums.ApplicationReviewDecision Decision { get; set; }
        public string? AdminNotes { get; set; }
        public long AdminId { get; set; }
    }

    public class UpdateServiceCenterStatusCommandHandler : IRequestHandler<UpdateServiceCenterStatusCommand, BaseResponse<long>>
    {
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceCenterPartneringApplication> _appRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceCenter> _scRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceCenterImage> _imageRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.User> _userRepo;
        private readonly GD1.Application.Interfaces.IEmailService _emailService;
        private readonly GD1.Application.Common.Interfaces.IPaymentService _paymentService;

        public UpdateServiceCenterStatusCommandHandler(
            IGenericRepository<GD1.Domain.Entities.ServiceCenterPartneringApplication> appRepo,
            IGenericRepository<GD1.Domain.Entities.ServiceCenter> scRepo,
            IGenericRepository<GD1.Domain.Entities.ServiceCenterImage> imageRepo,
            IGenericRepository<GD1.Domain.Entities.User> userRepo,
            GD1.Application.Interfaces.IEmailService emailService,
            GD1.Application.Common.Interfaces.IPaymentService paymentService)
        {
            _appRepo = appRepo;
            _scRepo = scRepo;
            _imageRepo = imageRepo;
            _userRepo = userRepo;
            _emailService = emailService;
            _paymentService = paymentService;
        }

        public async Task<BaseResponse<long>> Handle(UpdateServiceCenterStatusCommand cmd, CancellationToken ct)
        {
            var app = await _appRepo.GetByIdAsync(cmd.Id);
            if (app == null) return BaseResponse<long>.Fail("Service Center application not found.");

            if (app.Status == "Approved" || app.Status == "Rejected")
            {
                return BaseResponse<long>.Fail("This application has already been processed.");
            }

            if (cmd.Decision == GD1.Domain.Entities.Enums.ApplicationReviewDecision.Approved)
            {
                app.Status = "Approved";

                // Provision the actual Service Center
                var sc = new GD1.Domain.Entities.ServiceCenter
                {
                    AdminId = app.ApplicantId,
                    Name = app.Name,
                    OwnerName = app.OwnerName,
                    Email = app.Email,
                    PhoneNumber = app.PhoneNumber,
                    AddressLine = app.AddressLine,
                    City = app.City,
                    District = app.District,
                    State = app.State,
                    Country = app.Country,
                    PostalCode = app.PostalCode,
                    Latitude = app.Latitude,
                    Longitude = app.Longitude,
                    
                    OwnerIdProofUrl = app.OwnerIdProofUrl,

                    Status = "Approved",
                    IsVerified = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _scRepo.AddAsync(sc);

                var images = await _imageRepo.FindAsync(i => i.ApplicationId == app.Id);
                foreach (var img in images)
                {
                    img.ServiceCenterId = sc.Id;
                    await _imageRepo.UpdateAsync(img);
                }

                // Update the applicant's role to ServiceCenter
                var user = await _userRepo.GetByIdAsync(app.ApplicantId);
                if (user != null)
                {
                    user.Role = GD1.Domain.Entities.Enums.UserRole.ServiceCenter;
                    await _userRepo.UpdateAsync(user);

                    // Send approval email
                    string dashboardUrl = "https://grandautodepot.com/dashboard/service-center";
                    string emailBody = $@"
                        <h3>Congratulations!</h3>
                        <p>Your Service Center Partnering Application for <strong>{app.Name}</strong> has been approved and added to GD1.</p>
                        <p>You can now manage your Service Center using your GD1 account.</p>
                        <p><a href='{dashboardUrl}'>Click here to access your Service Center Dashboard</a></p>
                    ";
                    await _emailService.SendAsync(user.Email, "GD1 Service Center Approved", emailBody);
                }
            }
            else if (cmd.Decision == GD1.Domain.Entities.Enums.ApplicationReviewDecision.Rejected)
            {
                app.Status = "Rejected";

                if (!string.IsNullOrEmpty(app.FeeTransactionId))
                {
                    try
                    {
                        var refundResult = await _paymentService.RefundPaymentAsync(app.FeeTransactionId, app.ApplicationFee);
                        if (!refundResult.IsSuccess)
                        {
                            cmd.AdminNotes += $" [Note: Automatic refund of ₹{app.ApplicationFee} failed due to invalid payment ID. Please process manually.]";
                        }
                    }
                    catch (Exception ex)
                    {
                        cmd.AdminNotes += $" [Note: Automatic refund error: {ex.Message}]";
                    }
                }
            }
            else
            {
                return BaseResponse<long>.Fail("Invalid decision.");
            }

            app.AdminNotes = cmd.AdminNotes;

            await _appRepo.UpdateAsync(app);

            return BaseResponse<long>.Ok(app.Id, "Service Center status updated successfully.");
        }
    }
}

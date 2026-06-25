using GD1.Application.Common;
using GD1.Application.Common.Interfaces;
using GD1.Application.Features.FranchiseApplication.DTOs;
using GD1.Application.Interfaces.Repositories;
using GD1.Domain.Entities;
using GD1.Domain.Entities.Enums;
using GD1.Application.Interfaces.Services;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Application.Features.FranchiseApplication.Commands
{
    public class SubmitApplicationCommand : IRequest<BaseResponse<long>>
    {
        public long ApplicantId { get; set; }
        public GD1.Domain.Entities.Enums.ApplicationType ApplicationType { get; set; } = GD1.Domain.Entities.Enums.ApplicationType.Franchise;
        public string BusinessName { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string AddressLine { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string? PostalCode { get; set; }
        public decimal PricePerDay { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime PreferredInspectionDate { get; set; }

        public string? BusinessRegistrationUrl { get; set; }
        public string? LicenseDocumentUrl { get; set; }
        public string? OwnerIdProofUrl { get; set; }
        public string? PropertyProofUrl { get; set; }

        public bool HasCCTV { get; set; }
        public bool HasSecurity { get; set; }
        public bool HasFireSafety { get; set; }
        public bool HasWorkshop { get; set; }
        public bool HasWashingArea { get; set; }

        public List<string> PropertyImages { get; set; } = [];
        public List<FranchiseSlotRequest> Slots { get; set; } = [];

        public string RazorpayOrderId { get; set; } = string.Empty;
        public string RazorpayPaymentId { get; set; } = string.Empty;
        public string RazorpaySignature { get; set; } = string.Empty;
    }

    public class FranchiseSlotRequest
    {
        public string SlotNumber { get; set; } = string.Empty;
        public double SquareFeet { get; set; }
        public double HeightFeet { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
    }

    public class SubmitApplicationCommandHandler : IRequestHandler<SubmitApplicationCommand, BaseResponse<long>>
    {
        private readonly IGenericRepository<GD1.Domain.Entities.FranchiseApplication> _appRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceCenterPartneringApplication> _scRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceCenterImage> _scImageRepo;
        private readonly IGenericRepository<FranchiseSlot> _slotRepo;
        private readonly IGenericRepository<PropertyImage> _imageRepo;
        private readonly INotificationService _notificationService;
        private readonly IGenericRepository<GD1.Domain.Entities.User> _userRepo;
        private readonly IPaymentService _paymentService;

        public SubmitApplicationCommandHandler(
            IGenericRepository<GD1.Domain.Entities.FranchiseApplication> appRepo,
            IGenericRepository<GD1.Domain.Entities.ServiceCenterPartneringApplication> scRepo,
            IGenericRepository<GD1.Domain.Entities.ServiceCenterImage> scImageRepo,
            IGenericRepository<FranchiseSlot> slotRepo,
            IGenericRepository<PropertyImage> imageRepo,
            INotificationService notificationService,
            IGenericRepository<GD1.Domain.Entities.User> userRepo,
            IPaymentService paymentService)
        {
            _appRepo = appRepo;
            _scRepo = scRepo;
            _scImageRepo = scImageRepo;
            _slotRepo = slotRepo;
            _imageRepo = imageRepo;
            _notificationService = notificationService;
            _userRepo = userRepo;
            _paymentService = paymentService;
        }

        public async Task<BaseResponse<long>> Handle(SubmitApplicationCommand cmd, CancellationToken ct)
        {
            if (cmd.ApplicationType == GD1.Domain.Entities.Enums.ApplicationType.Franchise)
            {
                if (cmd.PricePerDay <= 0)
                    return BaseResponse<long>.Fail("Price per day must be greater than 0.");

                if (cmd.Slots == null || cmd.Slots.Count == 0)
                    return BaseResponse<long>.Fail("At least one garage (slot) must be added.");

                foreach (var slot in cmd.Slots)
                {
                    if (string.IsNullOrEmpty(slot.ImageUrl))
                        return BaseResponse<long>.Fail($"Image is required for garage {slot.SlotNumber}.");
                    if (slot.SquareFeet <= 0 || slot.HeightFeet <= 0)
                        return BaseResponse<long>.Fail($"Valid dimensions are required for garage {slot.SlotNumber}.");
                }
            }

            // Verify Razorpay Payment Signature
            bool isPaymentValid = false;
            try
            {
                isPaymentValid = _paymentService.VerifySignature(cmd.RazorpayOrderId, cmd.RazorpayPaymentId, cmd.RazorpaySignature);
            }
            catch (Exception ex)
            {
                return BaseResponse<long>.Fail("Payment verification error: " + ex.Message);
            }

            if (!isPaymentValid)
            {
                return BaseResponse<long>.Fail("Payment signature verification failed. Application cannot be submitted.");
            }

            // Dummy geocoding implementation based on city for now.
            // TODO: Integrate proper map/geocoding API later.
            double lat = 11.2588; // default to Calicut
            double lng = 75.7804;
            if (cmd.City.Equals("Kochi", StringComparison.OrdinalIgnoreCase)) { lat = 9.9312; lng = 76.2673; }
            else if (cmd.City.Equals("Trivandrum", StringComparison.OrdinalIgnoreCase) || cmd.City.Equals("Thiruvananthapuram", StringComparison.OrdinalIgnoreCase)) { lat = 8.5241; lng = 76.9366; }
            else if (cmd.City.Equals("Bangalore", StringComparison.OrdinalIgnoreCase) || cmd.City.Equals("Bengaluru", StringComparison.OrdinalIgnoreCase)) { lat = 12.9716; lng = 77.5946; }

            if (cmd.ApplicationType == GD1.Domain.Entities.Enums.ApplicationType.ServiceCenter)
            {
                var scApp = new GD1.Domain.Entities.ServiceCenterPartneringApplication
                {
                    ApplicantId = cmd.ApplicantId,
                    Name = cmd.BusinessName,
                    OwnerName = cmd.OwnerName,
                    Email = cmd.ContactEmail,
                    PhoneNumber = cmd.PhoneNumber,
                    AddressLine = cmd.AddressLine,
                    City = cmd.City,
                    District = cmd.City, // Using City as District for now
                    State = cmd.State,
                    PostalCode = cmd.PostalCode,
                    Latitude = lat,
                    Longitude = lng,
                    Status = "PendingReview",
                    OwnerIdProofUrl = cmd.OwnerIdProofUrl,
                    ApplicationFee = 2000m,
                    FeeStatus = "Paid",
                    FeeTransactionId = cmd.RazorpayPaymentId,
                    PreferredInspectionDate = cmd.PreferredInspectionDate,
                    PricePerDay = cmd.PricePerDay,
                    CreatedAt = DateTime.UtcNow,
                };

                await _scRepo.AddAsync(scApp);

                foreach (var imgUrl in cmd.PropertyImages)
                {
                    await _scImageRepo.AddAsync(new GD1.Domain.Entities.ServiceCenterImage
                    {
                        ApplicationId = scApp.Id,
                        ImageUrl = imgUrl
                    });
                }
                
                return BaseResponse<long>.Ok(scApp.Id, "Service Center application submitted successfully.");
            }

            var application = new GD1.Domain.Entities.FranchiseApplication
            {
                ApplicantId = cmd.ApplicantId,
                ApplicationType = cmd.ApplicationType,
                BusinessName = cmd.BusinessName,
                OwnerName = cmd.OwnerName,
                ContactEmail = cmd.ContactEmail,
                PhoneNumber = cmd.PhoneNumber,
                AddressLine = cmd.AddressLine,
                City = cmd.City,
                State = cmd.State,
                PostalCode = cmd.PostalCode,
                Latitude = lat,
                Longitude = lng,
                PreferredInspectionDate = cmd.PreferredInspectionDate,
                BusinessRegistrationUrl = cmd.BusinessRegistrationUrl,
                LicenseDocumentUrl = cmd.LicenseDocumentUrl,
                OwnerIdProofUrl = cmd.OwnerIdProofUrl,
                PropertyProofUrl = cmd.PropertyProofUrl,
                PricePerDay = cmd.PricePerDay,
                Status = FranchiseStatus.Pending,
                HasCCTV = cmd.HasCCTV,
                HasSecurity = cmd.HasSecurity,
                HasFireSafety = cmd.HasFireSafety,
                HasWorkshop = cmd.HasWorkshop,
                HasWashingArea = cmd.HasWashingArea,
                ApplicationFee = 2000m,
                FeeStatus = "Paid",
                FeeTransactionId = cmd.RazorpayPaymentId,
                CreatedAt = DateTime.UtcNow
            };

            await _appRepo.AddAsync(application);

            bool isFirst = true;
            foreach (var imgUrl in cmd.PropertyImages)
            {
                await _imageRepo.AddAsync(new PropertyImage
                {
                    ApplicationId = application.Id,
                    ImageUrl = imgUrl,
                    Label = isFirst ? "Property Main" : "Additional Image",
                    UploadedBy = "Owner",
                    IsMain = isFirst
                });
                isFirst = false;
            }

            foreach (var s in cmd.Slots)
            {
                await _slotRepo.AddAsync(new FranchiseSlot
                {
                    ApplicationId = application.Id,
                    SlotNumber = s.SlotNumber,
                    SquareFeet = s.SquareFeet,
                    HeightFeet = s.HeightFeet,
                    ImageUrl = s.ImageUrl
                });
            }

            // Send Notifications
            try
            {
                // Notify Applicant
                await _notificationService.SendAsync(
                    userId: cmd.ApplicantId,
                    title: "Application Submitted",
                    body: $"Your franchise application for {cmd.BusinessName} has been received and is under review.",
                    actionType: "TrackApplication",
                    referenceId: application.Id);

                // Notify Admins
                var admins = await _userRepo.FindAsync(u => u.Role == UserRole.GD1Admin);
                if (admins.Any())
                {
                    await _notificationService.SendToManyAsync(
                        userIds: admins.Select(a => a.Id),
                        title: "New Franchise Application",
                        body: $"{cmd.OwnerName} from {cmd.City} submitted a new franchise application.",
                        actionType: "ReviewFranchise",
                        referenceId: application.Id);
                }
            }
            catch { /* Do not fail the request if notification fails */ }

            return BaseResponse<long>.Ok(application.Id, "Application submitted successfully.");
        }
    }
}

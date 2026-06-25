using GD1.Application.Common;
using GD1.Domain.Interfaces;
using GD1.Domain.Entities;
using GD1.Domain.Entities.Enums;
using GD1.Application.Interfaces.Repositories;
using GD1.Application.Interfaces.Services;
using GD1.Application.Common.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Application.Features.GD1Admin.Commands
{
    public class UpdateApplicationStatusCommand : IRequest<BaseResponse<string>>
    {
        public long Id { get; set; }
        public ApplicationReviewDecision Decision { get; set; }
        public string? AdminNotes { get; set; }
        public long AdminId { get; set; }
    }

    public class UpdateApplicationStatusCommandHandler : IRequestHandler<UpdateApplicationStatusCommand, BaseResponse<string>>
    {
        private readonly IGenericRepository<GD1.Domain.Entities.FranchiseApplication> _repo;
        private readonly IGenericRepository<InspectionAssignment> _assignRepo;
        private readonly IGenericRepository<InspectionReport> _reportRepo;
        private readonly IGenericRepository<VehicleStorageProperty> _propertyRepo;
        private readonly IGenericRepository<VehicleStorageSlot> _slotRepo;
        private readonly IGenericRepository<User> _userRepo;
        private readonly INotificationService _notificationService;
        private readonly IPaymentService _paymentService;

        public UpdateApplicationStatusCommandHandler(
            IGenericRepository<GD1.Domain.Entities.FranchiseApplication> repo,
            IGenericRepository<InspectionAssignment> assignRepo,
            IGenericRepository<InspectionReport> reportRepo,
            IGenericRepository<VehicleStorageProperty> propertyRepo,
            IGenericRepository<VehicleStorageSlot> slotRepo,
            IGenericRepository<User> userRepo,
            INotificationService notificationService,
            IPaymentService paymentService)
        {
            _repo = repo;
            _assignRepo = assignRepo;
            _reportRepo = reportRepo;
            _propertyRepo = propertyRepo;
            _slotRepo = slotRepo;
            _userRepo = userRepo;
            _notificationService = notificationService;
            _paymentService = paymentService;
        }

        public async Task<BaseResponse<string>> Handle(UpdateApplicationStatusCommand cmd, CancellationToken ct)
        {
            var app = (await _repo.FindAsync(a => a.Id == cmd.Id, "Slots")).FirstOrDefault();
            if (app == null) return BaseResponse<string>.Fail("Application not found.");

            var targetStatus = cmd.Decision == ApplicationReviewDecision.Approved 
                ? FranchiseStatus.Approved 
                : FranchiseStatus.Rejected;

            if (targetStatus == FranchiseStatus.Approved)
            {
                if (app.ApplicationType == ApplicationType.Franchise)
                {
                    // Verify if inspection is completed
                    var assignments = await _assignRepo.FindAsync(a => a.ApplicationId == app.Id && a.Status == "Completed");
                    if (!assignments.Any())
                        return BaseResponse<string>.Fail("Cannot approve Franchise without a completed inspection.");

                    // 1. Create Property (Franchise)
                    var storageProperty = new VehicleStorageProperty
                    {
                        LotOwnerId = app.ApplicantId,
                        LotCode = $"GD1-{app.State[..2].ToUpper()}-{app.Id:D4}",
                        Name = app.BusinessName,
                        Description = $"{app.ApplicationType} Private Garage Property",
                        AddressLine = app.AddressLine,
                        City = app.City,
                        State = app.State,
                        Country = app.Country,
                        Latitude = app.Latitude,
                        Longitude = app.Longitude,
                        Status = "Active",
                        HasCCTV = app.HasCCTV,
                        HasSecurity = app.HasSecurity,
                        HasFireSafety = app.HasFireSafety,
                        HasWorkshopBay = app.HasWorkshop,
                        HasWashingArea = app.HasWashingArea,
                        PricePerDay = 600,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _propertyRepo.AddAsync(storageProperty);

                    // 2. Create Slots from Application
                    foreach (var s in app.Slots)
                    {
                        await _slotRepo.AddAsync(new VehicleStorageSlot
                        {
                            PropertyId = storageProperty.Id,
                            SlotNumber = s.SlotNumber,
                            SlotType = "Private Garage",
                            IsOccupied = false,
                            SquareFeet = s.SquareFeet,
                            HeightFeet = s.HeightFeet,
                            ImageUrl = s.ImageUrl,
                            CreatedAt = DateTime.UtcNow
                        });
                    }

                    // Promote Role and Sync Phone Number
                    var user = await _userRepo.GetByIdAsync(app.ApplicantId);
                    if (user != null)
                    {
                        if (user.Role == UserRole.VehicleOwner)
                            user.Role = UserRole.LotOwner;
                        
                        if (string.IsNullOrEmpty(user.PhoneNumber))
                            user.PhoneNumber = app.PhoneNumber;

                        await _userRepo.UpdateAsync(user);
                    }
                } // End if Franchise
                else if (app.ApplicationType == ApplicationType.ServiceCenter)
                {
                    // No property/slots to create for Service Center approval in this context
                    var user = await _userRepo.GetByIdAsync(app.ApplicantId);
                    if (user != null)
                    {
                        if (user.Role == UserRole.VehicleOwner)
                            user.Role = UserRole.LotOwner;
                        
                        if (string.IsNullOrEmpty(user.PhoneNumber))
                            user.PhoneNumber = app.PhoneNumber;

                        await _userRepo.UpdateAsync(user);
                    }
                }
            }
            else if (targetStatus == FranchiseStatus.Rejected)
            {
                // Refund logic
                if (!string.IsNullOrEmpty(app.FeeTransactionId))
                {
                    decimal refundAmount = 2000m;
                    if (app.ApplicationType == ApplicationType.Franchise)
                    {
                        var hasInspection = await _assignRepo.FindAsync(a => a.ApplicationId == app.Id && a.Status == "Completed");
                        if (hasInspection.Any())
                        {
                            refundAmount = 1000m;
                        }
                    }

                    try
                    {
                        var refundResult = await _paymentService.RefundPaymentAsync(app.FeeTransactionId, refundAmount);
                        if (!refundResult.IsSuccess)
                        {
                            cmd.AdminNotes += $" [Note: Automatic refund of ₹{refundAmount} failed due to invalid payment ID. Please process manually.]";
                        }
                        else
                        {
                            app.IsRefunded = true;
                            app.RefundTransactionId = refundResult.RefundId;
                        }
                    }
                    catch (Exception ex)
                    {
                        cmd.AdminNotes += $" [Note: Automatic refund error: {ex.Message}]";
                    }
                }
            }

            app.Status = targetStatus;
            app.AdminNotes = cmd.AdminNotes;
            app.ReviewedBy = cmd.AdminId;
            app.ReviewedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(app);

            // Send notification
            try
            {
                if (targetStatus == FranchiseStatus.Approved)
                {
                    await _notificationService.SendAsync(
                        userId: app.ApplicantId,
                        title: "Application Approved!",
                        body: "Congratulations! You have successfully partnered with GD1 as a Franchise owner.",
                        actionType: "ViewDashboard",
                        referenceId: app.Id);
                }
                else if (targetStatus == FranchiseStatus.Rejected)
                {
                    await _notificationService.SendAsync(
                        userId: app.ApplicantId,
                        title: "Application Rejected",
                        body: $"Your application was rejected. Reason: {cmd.AdminNotes}",
                        actionType: "SeeReason",
                        referenceId: app.Id);
                }
            }
            catch { /* Do not fail the request if notification fails */ }

            return BaseResponse<string>.Ok(string.Empty, $"Application {targetStatus}.");
        }
    }
}

using GD1.Application.Common;
using GD1.Domain.Interfaces;
using GD1.Domain.Entities;
using GD1.Domain.Entities.Enums;
using GD1.Application.Interfaces.Repositories;
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
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceCenter> _scRepo;

        public UpdateApplicationStatusCommandHandler(
            IGenericRepository<GD1.Domain.Entities.FranchiseApplication> repo,
            IGenericRepository<InspectionAssignment> assignRepo,
            IGenericRepository<InspectionReport> reportRepo,
            IGenericRepository<VehicleStorageProperty> propertyRepo,
            IGenericRepository<VehicleStorageSlot> slotRepo,
            IGenericRepository<User> userRepo,
            IGenericRepository<GD1.Domain.Entities.ServiceCenter> scRepo)
        {
            _repo = repo;
            _assignRepo = assignRepo;
            _reportRepo = reportRepo;
            _propertyRepo = propertyRepo;
            _slotRepo = slotRepo;
            _userRepo = userRepo;
            _scRepo = scRepo;
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
                // Verify if inspection is completed
                var assignments = await _assignRepo.FindAsync(a => a.ApplicationId == app.Id && a.Status == "Completed");
                if (!assignments.Any())
                    return BaseResponse<string>.Fail("Cannot approve without a completed inspection.");

                if (app.ApplicationType == ApplicationType.ServiceCenter)
                {
                    // Provision Service Center
                    var sc = new GD1.Domain.Entities.ServiceCenter
                    {
                        AdminId = app.ApplicantId,
                        Name = app.BusinessName,
                        PhoneNumber = app.PhoneNumber,
                        AddressLine = app.AddressLine,
                        City = app.City,
                        State = app.State,
                        Email = app.ContactEmail,
                        Latitude = app.Latitude,
                        Longitude = app.Longitude,
                        OemCertificateUrl = app.OemCertificateUrl,
                        SupportedBrand = app.SupportedBrand,
                        IsActive = true
                    };
                    await _scRepo.AddAsync(sc);

                    // Promote Role
                    var user = await _userRepo.GetByIdAsync(app.ApplicantId);
                    if (user != null)
                    {
                        if (user.Role == UserRole.VehicleOwner || user.Role == UserRole.Agent)
                            user.Role = UserRole.ServiceCenter;
                        
                        if (string.IsNullOrEmpty(user.PhoneNumber))
                            user.PhoneNumber = app.PhoneNumber;

                        await _userRepo.UpdateAsync(user);
                    }
                }
                else
                {
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
                }
            }

            app.Status = targetStatus;
            app.AdminNotes = cmd.AdminNotes;
            app.ReviewedBy = cmd.AdminId;
            app.ReviewedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(app);
            return BaseResponse<string>.Ok(string.Empty, $"Application {targetStatus}.");
        }
    }
}

using GD1.Application.Common;
using GD1.Application.Features.FranchiseApplication.DTOs;
using GD1.Application.Interfaces.Repositories;
using GD1.Domain.Entities;
using GD1.Domain.Entities.Enums;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Application.Features.GD1Admin.Commands
{
    public class ReviewInspectionCommand : IRequest<BaseResponse<string>>
    {
        public long ReportId { get; set; }
        public InspectionDecision Decision { get; set; }
        public string? Remarks { get; set; }
        public long AdminId { get; set; }
    }

    public class ReviewInspectionCommandHandler : IRequestHandler<ReviewInspectionCommand, BaseResponse<string>>
    {
        private readonly IGenericRepository<InspectionReport> _reportRepo;
        private readonly IGenericRepository<InspectionAssignment> _assignRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.FranchiseApplication> _appRepo;
        private readonly IGenericRepository<VehicleStorageProperty> _propertyRepo;
        private readonly IGenericRepository<VehicleStorageSlot> _slotRepo;

        public ReviewInspectionCommandHandler(
            IGenericRepository<InspectionReport> reportRepo,
            IGenericRepository<InspectionAssignment> assignRepo,
            IGenericRepository<GD1.Domain.Entities.FranchiseApplication> appRepo,
            IGenericRepository<VehicleStorageProperty> propertyRepo,
            IGenericRepository<VehicleStorageSlot> slotRepo)
        {
            _reportRepo = reportRepo;
            _assignRepo = assignRepo;
            _appRepo = appRepo;
            _propertyRepo = propertyRepo;
            _slotRepo = slotRepo;
        }

        public async Task<BaseResponse<string>> Handle(ReviewInspectionCommand cmd, CancellationToken ct)
        {
            var report = (await _reportRepo.FindAsync(r => r.Id == cmd.ReportId, "SlotVerifications")).FirstOrDefault();
            if (report == null) return BaseResponse<string>.Fail("Inspection report not found.");

            var assignment = await _assignRepo.GetByIdAsync(report.AssignmentId);
            if (assignment == null) return BaseResponse<string>.Fail("Assignment not found.");
            
            var app = (await _appRepo.FindAsync(a => a.Id == assignment.ApplicationId, "Slots")).FirstOrDefault();
            if (app == null) return BaseResponse<string>.Fail("Application not found.");

            report.AdminDecision = cmd.Decision;
            report.AdminRemarks = cmd.Remarks;
            report.UpdatedAt = DateTime.UtcNow;

            if (cmd.Decision == InspectionDecision.Approved)
            {
                app.Status = FranchiseStatus.Approved;
                
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
                    PricePerDay = app.PricePerDay,
                    AverageRating = 0,
                    TotalReviews = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _propertyRepo.AddAsync(storageProperty);

                var verifiedSlotMap = report.SlotVerifications.ToDictionary(s => s.SlotNumber);
                var proposedSlotMap = app.Slots.ToDictionary(s => s.SlotNumber);

                var allSlotNumbers = proposedSlotMap.Keys.Union(verifiedSlotMap.Keys).OrderBy(s => s);

                foreach (var slotNum in allSlotNumbers)
                {
                    var pSlot = proposedSlotMap.GetValueOrDefault(slotNum);
                    var vSlot = verifiedSlotMap.GetValueOrDefault(slotNum);

                    if (vSlot == null || !vSlot.IsVerified) continue;

                    await _slotRepo.AddAsync(new VehicleStorageSlot
                    {
                        PropertyId = storageProperty.Id,
                        SlotNumber = slotNum,
                        SlotType = "Private Garage",
                        IsOccupied = false,
                        SquareFeet = vSlot.SquareFeet,
                        HeightFeet = vSlot.HeightFeet,
                        ImageUrl = vSlot.ImageUrl ?? pSlot?.ImageUrl,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
            }
            else
            {
                app.Status = FranchiseStatus.Rejected;
                app.AdminNotes = cmd.Remarks;
            }

            await _reportRepo.UpdateAsync(report);
            await _appRepo.UpdateAsync(app);

            return BaseResponse<string>.Ok(string.Empty, $"Inspection {cmd.Decision}.");
        }
    }
}

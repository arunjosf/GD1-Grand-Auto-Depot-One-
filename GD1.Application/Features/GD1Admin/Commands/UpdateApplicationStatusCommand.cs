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
        private readonly IGenericRepository<InspectionItem> _itemRepo;

        public UpdateApplicationStatusCommandHandler(
            IGenericRepository<GD1.Domain.Entities.FranchiseApplication> repo,
            IGenericRepository<InspectionAssignment> assignRepo,
            IGenericRepository<InspectionReport> reportRepo,
            IGenericRepository<InspectionItem> itemRepo,
            IGenericRepository<LotUnit> unitRepo,
            IGenericRepository<StorageLot> lotRepo,
            IFranchiseReadRepository franchiseRead)
        {
            _repo = repo;
            _assignRepo = assignRepo;
            _reportRepo = reportRepo;
            _itemRepo = itemRepo;
            _unitRepo = unitRepo;
            _lotRepo = lotRepo;
            _franchiseRead = franchiseRead;
        }

        private readonly IGenericRepository<LotUnit> _unitRepo;
        private readonly IGenericRepository<StorageLot> _lotRepo;
        private readonly IFranchiseReadRepository _franchiseRead;

        public async Task<BaseResponse<string>> Handle(UpdateApplicationStatusCommand cmd, CancellationToken ct)
        {
            var app = await _repo.GetByIdAsync(cmd.Id);
            if (app == null) return BaseResponse<string>.Fail("Application not found.");

            // Map Decision to FranchiseStatus
            var targetStatus = cmd.Decision == ApplicationReviewDecision.Approved 
                ? FranchiseStatus.Approved 
                : FranchiseStatus.Rejected;

            if (targetStatus == FranchiseStatus.Rejected)
            {
                // Check if an inspection has already been assigned
                var assignments = await _assignRepo.FindAsync(a => a.ApplicationId == app.Id);
                
                if (!assignments.Any())
                {
                    // Case 1: Rejected BEFORE inspection assignment -> Hard Delete
                    await _repo.DeleteAsync(app);
                    return BaseResponse<string>.Ok(string.Empty, "Application rejected and data removed permanently.");
                }
                else
                {
                    // Case 2: Rejected AFTER/DURING inspection (though usually handled in ReviewInspectionCommand)
                    // -> Soft Delete
                    app.Status = FranchiseStatus.Rejected;
                    app.IsDeleted = true;
                    app.AdminNotes = cmd.AdminNotes;
                    app.ReviewedBy = cmd.AdminId;
                    app.ReviewedAt = DateTime.UtcNow;
                    await _repo.UpdateAsync(app);
                    return BaseResponse<string>.Ok(string.Empty, "Application rejected (Soft Delete applied as inspection records exist).");
                }
            }

            if (targetStatus == FranchiseStatus.Approved)
            {
                // Check if there is at least one completed inspection report with all items verified
                var assignments = await _assignRepo.FindAsync(a => a.ApplicationId == app.Id && a.Status == "Completed");
                if (!assignments.Any())
                {
                    return BaseResponse<string>.Fail("Cannot approve. No completed inspection report found for this application.");
                }

                // Check for unverified items in the latest report
                var allAssignments = await _assignRepo.FindAsync(a => a.ApplicationId == app.Id && a.Status == "Completed");
                var assignmentIds = allAssignments.Select(a => a.Id).ToList();

                foreach (var assignId in assignmentIds)
                {
                    var reports = await _reportRepo.FindAsync(r => r.AssignmentId == assignId);
                    foreach (var report in reports)
                    {
                        if (!report.IsVerified)
                        {
                            return BaseResponse<string>.Fail($"Cannot approve. Inspection report for assignment '{assignId}' has unverified items.");
                        }
                    }
                }

                // If we reached here, ALL inspections are verified.
                // CREATE STORAGE LOTS (Copied from ReviewInspection logic)
                var units = await _franchiseRead.GetLotUnitsByApplicationIdAsync(app.Id);
                foreach (var unit in units)
                {
                    var lotCode = $"GD1-{app.State[..2].ToUpper()}-{unit.Id:D4}";
                    
                    // Check if already created to avoid duplicates
                    var existingLots = await _lotRepo.FindAsync(l => l.LotUnitId == unit.Id);
                    if (!existingLots.Any())
                    {
                        var storageLot = new StorageLot
                        {
                            LotOwnerId = app.ApplicantId,
                            LotUnitId = unit.Id,
                            LotCode = lotCode,
                            Name = $"{app.BusinessName} - {unit.Label}",
                            AddressLine = app.AddressLine,
                            City = app.City,
                            State = app.State,
                            Country = app.Country,
                            Latitude = app.Latitude,
                            Longitude = app.Longitude,
                            TotalSlots = unit.Capacity,
                            Tier = unit.Tier,
                            Status = "Active",
                            HasCCTV = unit.HasCCTV,
                            HasWorkshopBay = unit.HasWorkshop,
                            HasWashingArea = unit.HasWashingArea,
                            HasSecurity = unit.HasSecurity,
                            HasFireSafety = unit.HasFireSafety,
                            ExtraFacilities = unit.ExtraFacilities
                        };
                        await _lotRepo.AddAsync(storageLot);
                    }

                    var unitEntity = await _unitRepo.GetByIdAsync(unit.Id);
                    if (unitEntity != null)
                    {
                        unitEntity.Status = FranchiseStatus.Approved;
                        unitEntity.AssignedLotCode = lotCode;
                        await _unitRepo.UpdateAsync(unitEntity);
                    }
                }
            }

            app.Status = targetStatus;
            app.AdminNotes = cmd.AdminNotes;
            app.ReviewedBy = cmd.AdminId;
            app.ReviewedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(app);
            return BaseResponse<string>.Ok(string.Empty, "Status updated successfully.");
        }
    }
}

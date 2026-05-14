using GD1.Application.Common;
using GD1.Application.Interfaces.Repositories;
using GD1.Domain.Interfaces;
using GD1.Domain.Entities;
using GD1.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;
using FluentValidation;

namespace GD1.Application.Features.GD1Admin.Commands
{
    public class ReviewInspectionCommand : IRequest<BaseResponse<string>>
    {
        public long ReportId { get; set; }
        public long AdminId { get; set; }
        public InspectionDecision Decision { get; set; }
        public string? AdminRemarks { get; set; }
    }

    public class ReviewInspectionCommandValidator : AbstractValidator<ReviewInspectionCommand>
    {
        public ReviewInspectionCommandValidator()
        {
            RuleFor(x => x.ReportId).GreaterThan(0);
            RuleFor(x => x.AdminId).GreaterThan(0);
            RuleFor(x => x.Decision).IsInEnum();
        }
    }

    public class ReviewInspectionCommandHandler : IRequestHandler<ReviewInspectionCommand, BaseResponse<string>>
    {
        private readonly IGenericRepository<InspectionReport> _reportRepo;
        private readonly IGenericRepository<InspectionAssignment> _assignRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.FranchiseApplication> _appRepo;
        private readonly IGenericRepository<LotUnit> _unitRepo;
        private readonly IGenericRepository<StorageLot> _lotRepo;
        private readonly IFranchiseReadRepository _franchiseRead;
        private readonly IGenericRepository<InspectionItem> _itemRepo;

        public ReviewInspectionCommandHandler(
            IGenericRepository<InspectionReport> reportRepo,
            IGenericRepository<InspectionAssignment> assignRepo,
            IGenericRepository<GD1.Domain.Entities.FranchiseApplication> appRepo,
            IGenericRepository<LotUnit> unitRepo,
            IGenericRepository<StorageLot> lotRepo,
            IFranchiseReadRepository franchiseRead,
            IGenericRepository<InspectionItem> itemRepo)
        {
            _reportRepo = reportRepo;
            _assignRepo = assignRepo;
            _appRepo = appRepo;
            _unitRepo = unitRepo;
            _lotRepo = lotRepo;
            _franchiseRead = franchiseRead;
            _itemRepo = itemRepo;
        }

        public async Task<BaseResponse<string>> Handle(ReviewInspectionCommand cmd, CancellationToken cancellationToken)
        {
            var report = await _reportRepo.GetByIdAsync(cmd.ReportId);
            if (report is null) throw new KeyNotFoundException("Inspection report not found.");

            var assignment = await _assignRepo.GetByIdAsync(report.AssignmentId);
            if (assignment == null) throw new KeyNotFoundException("Associated assignment not found.");

            if (cmd.Decision == InspectionDecision.Approved)
            {
                // Check if all items are verified
                var items = await _itemRepo.FindAsync(i => i.ReportId == cmd.ReportId);
                var itemList = items.ToList();

                if (!itemList.Any())
                {
                    return BaseResponse<string>.Fail("Cannot approve. No inspection items found in the report.");
                }

                if (itemList.Any(i => !i.IsVerified))
                {
                    return BaseResponse<string>.Fail("Cannot approve. Some inspection items are not verified by the agent.");
                }

                var app = await _appRepo.GetByIdAsync(assignment.ApplicationId);
                var units = await _franchiseRead.GetLotUnitsByApplicationIdAsync(assignment.ApplicationId);

                if (app is not null)
                {
                    foreach (var unit in units)
                    {
                        var lotCode = $"GD1-{app.State[..2].ToUpper()}-{unit.Id:D4}";

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
                            Status = "Active", // StorageLot status is still string in entity? Let me check. Wait, I didn't change StorageLot.Status.
                            HasCCTV = unit.HasCCTV,
                            HasWorkshopBay = unit.HasWorkshop,
                            HasWashingArea = unit.HasWashingArea,
                            HasSecurity = unit.HasSecurity,
                            HasFireSafety = unit.HasFireSafety,
                            ExtraFacilities = unit.ExtraFacilities
                        };

                        await _lotRepo.AddAsync(storageLot);

                        var unitEntity = await _unitRepo.GetByIdAsync(unit.Id);
                        if (unitEntity != null)
                        {
                            unitEntity.Status = FranchiseStatus.Approved;
                            unitEntity.AssignedLotCode = lotCode;
                            await _unitRepo.UpdateAsync(unitEntity);
                        }
                    }

                    app.Status = FranchiseStatus.Approved;
                    app.ReviewedBy = cmd.AdminId;
                    app.ReviewedAt = DateTime.UtcNow;
                    await _appRepo.UpdateAsync(app);
                }
            }
            else if (cmd.Decision == InspectionDecision.Rejected)
            {
                var app = await _appRepo.GetByIdAsync(assignment.ApplicationId);
                if (app is not null)
                {
                    app.Status = FranchiseStatus.Rejected;
                    app.IsDeleted = true;
                    app.ReviewedBy = cmd.AdminId;
                    app.ReviewedAt = DateTime.UtcNow;
                    await _appRepo.UpdateAsync(app);
                }
            }

            report.AdminDecision = cmd.Decision;
            report.AdminRemarks = cmd.AdminRemarks;
            report.DecisionAt = DateTime.UtcNow;

            await _reportRepo.UpdateAsync(report);

            return BaseResponse<string>.Ok(string.Empty, $"Inspection {cmd.Decision}d successfully.");
        }
    }
}

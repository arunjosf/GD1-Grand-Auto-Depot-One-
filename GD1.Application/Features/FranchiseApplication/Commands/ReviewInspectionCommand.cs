using GD1.Application.Common;
using GD1.Application.Interfaces.Repositories;
using GD1.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Application.Features.FranchiseApplication.Commands
{
    public class ReviewInspectionCommand
    {
        public long ReportId { get; set; }
        public long AdminId { get; set; }
        public string Decision { get; set; } = string.Empty;
        public string? AdminRemarks { get; set; }
    }

    public class ReviewInspectionCommandHandler
    {
        private readonly IGenericRepository<GD1.Domain.Entities.InspectionReport> _reportRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.FranchiseApplication> _appRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.LotUnit> _unitRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.StorageLot> _lotRepo;
        private readonly IFranchiseReadRepository _franchiseRead;

        public ReviewInspectionCommandHandler(
            IGenericRepository<GD1.Domain.Entities.InspectionReport> reportRepo,
            IGenericRepository<GD1.Domain.Entities.FranchiseApplication> appRepo,
            IGenericRepository<GD1.Domain.Entities.LotUnit> unitRepo,
            IGenericRepository<GD1.Domain.Entities.StorageLot> lotRepo,
            IFranchiseReadRepository franchiseRead)
        {
            _reportRepo = reportRepo;
            _appRepo = appRepo;
            _unitRepo = unitRepo;
            _lotRepo = lotRepo;
            _franchiseRead = franchiseRead;
        }

        public async Task<BaseResponse<string>> HandleAsync(ReviewInspectionCommand cmd)
        {
            var valid = new[] { "Approve", "Conditional", "Reject" };
            if (!valid.Contains(cmd.Decision))
                throw new InvalidOperationException(
                    "Decision must be Approve, Conditional, or Reject.");

            var report = await _reportRepo.GetByIdAsync(cmd.ReportId);
            if (report is null)
                throw new KeyNotFoundException("Inspection report not found.");

            if (report.Status != "Submitted")
                throw new InvalidOperationException(
                    "Report must be Submitted before review.");

            report.AdminDecision = cmd.Decision;
            report.AdminRemarks = cmd.AdminRemarks;
            report.DecisionAt = DateTime.UtcNow;
            report.Status = cmd.Decision switch
            {
                "Approve" => "Approved",
                "Conditional" => "Conditional",
                _ => "Rejected"
            };

            await _reportRepo.UpdateAsync(report);

            if (cmd.Decision == "Approve")
            {
                var unit = await _unitRepo.GetByIdAsync(report.LotUnitId);
                var app = await _appRepo.GetByIdAsync(report.ApplicationId);

                if (unit is not null && app is not null)
                {
                    var lotCode = $"GD1-{app.State[..2].ToUpper()}-{report.Id:D4}";

                    var storageLot = new GD1.Domain.Entities.StorageLot
                    {
                        LotOwnerId = app.ApplicantId,
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
                        HasSecurity = unit.HasSecurity
                    };

                    await _lotRepo.AddAsync(storageLot);

                    unit.Status = "Approved";
                    unit.AssignedLotCode = lotCode;
                    await _unitRepo.UpdateAsync(unit);

                    var allReports = await _franchiseRead
                        .GetReportsByApplicationIdAsync(app.Id);

                    if (allReports.All(r => r.Status == "Approved"))
                    {
                        app.Status = "Approved";
                        app.ReviewedBy = cmd.AdminId;
                        app.ReviewedAt = DateTime.UtcNow;
                        await _appRepo.UpdateAsync(app);
                    }
                }
            }

            return BaseResponse<string>.Ok(
                string.Empty, $"Inspection {cmd.Decision}d successfully.");
        }
    }
}

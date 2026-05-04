using GD1.Application.Common;
using GD1.Application.Features.FranchiseApplication.DTOs;
using GD1.Application.Interfaces.Repositories;
using GD1.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Application.Features.FranchiseApplication.Commands
{
    public class SubmitInspectionRequest
    {
        public string Passcode { get; set; } = string.Empty;
        public string ChecklistJson { get; set; } = "[]";
        public string? AgentRemarks { get; set; }
        public List<PropertyImageRequest> Images { get; set; } = [];
    }

    public class SubmitInspectionCommand
    {
        public string AccessToken { get; set; } = string.Empty;
        public SubmitInspectionRequest Request { get; set; } = null!;
    }

    public class SubmitInspectionCommandHandler
    {
        private readonly IGenericRepository<GD1.Domain.Entities.InspectionReport> _reportRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.PropertyImage> _imageRepo;
        private readonly IFranchiseReadRepository _franchiseRead;

        public SubmitInspectionCommandHandler(
            IGenericRepository<GD1.Domain.Entities.InspectionReport> reportRepo,
            IGenericRepository<GD1.Domain.Entities.PropertyImage> imageRepo,
            IFranchiseReadRepository franchiseRead)
        {
            _reportRepo = reportRepo;
            _imageRepo = imageRepo;
            _franchiseRead = franchiseRead;
        }

        public async Task<BaseResponse<string>> HandleAsync(SubmitInspectionCommand cmd)
        {
            var report = await _franchiseRead
                .GetReportByTokenAsync(cmd.AccessToken);

            if (report is null)
                throw new KeyNotFoundException("Inspection link is invalid.");

            if (report.ExpiryDate < DateTime.UtcNow)
                throw new InvalidOperationException(
                    "Inspection link has expired. Contact GD1 Admin.");

            if (report.Status == "Submitted")
                throw new InvalidOperationException(
                    "Inspection already submitted.");

            if (!BCrypt.Net.BCrypt.Verify(cmd.Request.Passcode, report.PasscodeHash))
                throw new UnauthorizedAccessException("Incorrect passcode.");

            if (report.StartedAt is null)
            {
                report.StartedAt = DateTime.UtcNow;
                report.Status = "InProgress";
                await _reportRepo.UpdateAsync(report);
            }

            foreach (var img in cmd.Request.Images)
            {
                await _imageRepo.AddAsync(new GD1.Domain.Entities.PropertyImage
                {
                    ApplicationId = report.ApplicationId,
                    LotUnitId = report.LotUnitId,
                    UploadedBy = "Agent",
                    Label = img.Label,
                    ImageUrl = img.ImageUrl,
                    Remark = img.Remark
                });
            }

            report.ChecklistJson = cmd.Request.ChecklistJson;
            report.AgentRemarks = cmd.Request.AgentRemarks;
            report.Status = "Submitted";
            report.CompletedDate = DateTime.UtcNow;

            await _reportRepo.UpdateAsync(report);

            return BaseResponse<string>.Ok(
                string.Empty, "Inspection submitted successfully.");
        }
    }
}

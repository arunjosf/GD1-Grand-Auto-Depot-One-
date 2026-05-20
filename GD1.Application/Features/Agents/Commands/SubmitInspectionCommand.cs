using GD1.Application.Common;
using GD1.Application.Features.FranchiseApplication.DTOs;
using GD1.Application.Interfaces.Repositories;
using GD1.Domain.Entities;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Application.Features.Agents.Commands
{
    public class SubmitInspectionCommand : IRequest<BaseResponse<string>>
    {
        public long AssignmentId { get; set; }
        public string OverallDescription { get; set; } = string.Empty;
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public long UserId { get; set; } // Added to match controller call

        public List<string> SiteImages { get; set; } = [];
        public List<SlotVerificationRequest> SlotVerifications { get; set; } = [];
    }

    public class SlotVerificationRequest
    {
        public string SlotNumber { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
        public double SquareFeet { get; set; }
        public double HeightFeet { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
    }

    public class SubmitInspectionCommandHandler : IRequestHandler<SubmitInspectionCommand, BaseResponse<string>>
    {
        private readonly IGenericRepository<InspectionAssignment> _assignRepo;
        private readonly IGenericRepository<InspectionReport> _reportRepo;
        private readonly IGenericRepository<InspectionSlotItem> _slotItemRepo;
        private readonly IGenericRepository<PropertyImage> _imageRepo;

        public SubmitInspectionCommandHandler(
            IGenericRepository<InspectionAssignment> assignRepo,
            IGenericRepository<InspectionReport> reportRepo,
            IGenericRepository<InspectionSlotItem> slotItemRepo,
            IGenericRepository<PropertyImage> imageRepo)
        {
            _assignRepo = assignRepo;
            _reportRepo = reportRepo;
            _slotItemRepo = slotItemRepo;
            _imageRepo = imageRepo;
        }

        public async Task<BaseResponse<string>> Handle(SubmitInspectionCommand cmd, CancellationToken ct)
        {
            var assignment = await _assignRepo.GetByIdAsync(cmd.AssignmentId);
            if (assignment == null) return BaseResponse<string>.Fail("Assignment not found.");

            var report = new InspectionReport
            {
                AssignmentId = cmd.AssignmentId,
                StartedAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow,
                OverallDescription = cmd.OverallDescription,
                Latitude = cmd.Latitude,
                Longitude = cmd.Longitude,
                CreatedAt = DateTime.UtcNow
            };

            await _reportRepo.AddAsync(report);

            foreach (var imgUrl in cmd.SiteImages)
            {
                await _imageRepo.AddAsync(new PropertyImage
                {
                    ApplicationId = assignment.ApplicationId,
                    ImageUrl = imgUrl,
                    Label = "Agent Site Visit",
                    UploadedBy = "Agent"
                });
            }

            foreach (var sv in cmd.SlotVerifications)
            {
                await _slotItemRepo.AddAsync(new InspectionSlotItem
                {
                    ReportId = report.Id,
                    SlotNumber = sv.SlotNumber,
                    IsVerified = sv.IsVerified,
                    SquareFeet = sv.SquareFeet,
                    HeightFeet = sv.HeightFeet,
                    ImageUrl = sv.ImageUrl
                });
            }

            assignment.Status = "Completed";
            await _assignRepo.UpdateAsync(assignment);

            return BaseResponse<string>.Ok(string.Empty, "Inspection report submitted.");
        }
    }
}

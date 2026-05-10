using GD1.Application.Common;
using GD1.Application.Features.FranchiseApplication.DTOs;
using GD1.Application.Interfaces.Repositories;
using GD1.Domain.Interfaces;
using GD1.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;
using FluentValidation;

namespace GD1.Application.Features.Agents.Commands
{
    public class SubmitInspectionCommand : IRequest<BaseResponse<string>>
    {
        public long AssignmentId { get; set; }
        public PropertyInspectionSubmission Request { get; set; } = null!;
        public long UserId { get; set; } 
    }

    public class SubmitInspectionCommandValidator : AbstractValidator<SubmitInspectionCommand>
    {
        public SubmitInspectionCommandValidator()
        {
            RuleFor(x => x.AssignmentId).GreaterThan(0);
            RuleFor(x => x.Request.Units).NotEmpty();
            RuleFor(x => x.Request.OverallDescription).NotEmpty();
            RuleForEach(x => x.Request.Units).ChildRules(unit =>
            {
                unit.RuleFor(u => u.LotUnitId).GreaterThan(0);
            });
        }
    }

    public class SubmitInspectionCommandHandler : IRequestHandler<SubmitInspectionCommand, BaseResponse<string>>
    {
        private readonly IGenericRepository<InspectionAssignment> _assignRepo;
        private readonly IGenericRepository<InspectionReport> _reportRepo;
        private readonly IGenericRepository<InspectionItem> _itemRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.PropertyImage> _imageRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.LotUnitImage> _unitImageRepo;
        private readonly IGenericRepository<Agent> _agentRepo;
        private readonly IGenericRepository<LotUnit> _unitRepo;

        public SubmitInspectionCommandHandler(
            IGenericRepository<InspectionAssignment> assignRepo,
            IGenericRepository<InspectionReport> reportRepo,
            IGenericRepository<InspectionItem> itemRepo,
            IGenericRepository<GD1.Domain.Entities.PropertyImage> imageRepo,
            IGenericRepository<GD1.Domain.Entities.LotUnitImage> unitImageRepo,
            IGenericRepository<Agent> agentRepo,
            IGenericRepository<LotUnit> unitRepo)
        {
            _assignRepo = assignRepo;
            _reportRepo = reportRepo;
            _itemRepo = itemRepo;
            _imageRepo = imageRepo;
            _unitImageRepo = unitImageRepo;
            _agentRepo = agentRepo;
            _unitRepo = unitRepo;
        }

        public async Task<BaseResponse<string>> Handle(SubmitInspectionCommand cmd, CancellationToken cancellationToken)
        {
            // 1. Security Check
            var agent = (await _agentRepo.FindAsync(a => a.UserId == cmd.UserId)).FirstOrDefault();
            if (agent == null) throw new UnauthorizedAccessException("Agent profile not found.");

            var assignment = await _assignRepo.GetByIdAsync(cmd.AssignmentId);
            if (assignment is null) throw new KeyNotFoundException("Assignment not found.");

            if (assignment.AgentId != agent.Id)
                throw new UnauthorizedAccessException("You are not authorized to submit an inspection for this assignment.");

            if (assignment.Status == "Completed")
                throw new InvalidOperationException("Inspection already submitted for this assignment.");

            // 1.1 Cleanup existing reports if any (handles partial submission retries)
            var existingReports = await _reportRepo.FindAsync(r => r.AssignmentId == assignment.Id);
            foreach (var existingReport in existingReports)
            {
                await _reportRepo.DeleteAsync(existingReport);
            }

            // 1.2 Validate LotUnits (Ensure they exist and belong to this application)
            var applicationUnits = await _unitRepo.FindAsync(u => u.FranchiseApplicationId == assignment.ApplicationId);
            var validUnitIds = applicationUnits.Select(u => u.Id).ToHashSet();

            foreach (var unitReq in cmd.Request.Units)
            {
                if (!validUnitIds.Contains(unitReq.LotUnitId))
                {
                    return BaseResponse<string>.Fail($"Submission failed: The LotUnitId '{unitReq.LotUnitId}' is incorrect. It either does not exist or is not associated with this franchise application. Please check your IDs in Swagger and try again.");
                }
            }

            // 2. Prepare the Inspection Report & Items
            // Using explicit linking to tracked entities to help EF Core's change tracker
            var report = new InspectionReport
            {
                AssignmentId = assignment.Id,
                Assignment = assignment, 
                StartedAt = DateTime.UtcNow, 
                CompletedAt = DateTime.UtcNow,
                OverallDescription = cmd.Request.OverallDescription,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            report.Items = cmd.Request.Units.Select(u => new InspectionItem
            {
                LotUnitId = u.LotUnitId,
                Report = report,
                TaskName = "Unit Verification",
                IsVerified = u.IsVerified,
                Remarks = u.Remarks,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }).ToList();

            // 3. Mark Assignment as Completed
            assignment.Status = "Completed";
            assignment.UpdatedAt = DateTime.UtcNow;

            // 4. Persist Report & Items & Assignment Status
            await _reportRepo.AddAsync(report);

            // 5. Process General Property Images
            foreach (var imgUrl in cmd.Request.PropertyImages)
            {
                if (string.IsNullOrEmpty(imgUrl)) continue;
                await _imageRepo.AddAsync(new PropertyImage
                {
                    ApplicationId = assignment.ApplicationId,
                    UploadedBy = "Agent",
                    Label = "Overall Property View",
                    ImageUrl = imgUrl,
                    IsMain = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            // 6. Process Per-Unit Images
            foreach (var unitReq in cmd.Request.Units)
            {
                foreach (var unitImgUrl in unitReq.UnitImages)
                {
                    if (string.IsNullOrEmpty(unitImgUrl)) continue;
                    await _unitImageRepo.AddAsync(new LotUnitImage
                    {
                        LotUnitId = unitReq.LotUnitId,
                        UploadedBy = "Agent",
                        ImageUrl = unitImgUrl,
                        Remark = "Inspection View",
                        IsMain = false,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
            }

            return BaseResponse<string>.Ok(string.Empty, "Inspection submitted successfully.");
        }
    }
}

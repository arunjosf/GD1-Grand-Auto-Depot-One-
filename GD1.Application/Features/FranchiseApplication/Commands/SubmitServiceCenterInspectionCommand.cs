using GD1.Application.Common;
using GD1.Application.Interfaces.Repositories;
using GD1.Domain.Entities;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.FranchiseApplication.Commands
{
    public class SubmitServiceCenterInspectionCommand : IRequest<BaseResponse<string>>
    {
        public long AgentId { get; set; }
        public long AssignmentId { get; set; }
        public string OverallDescription { get; set; } = string.Empty;
    }

    public class SubmitServiceCenterInspectionCommandHandler : IRequestHandler<SubmitServiceCenterInspectionCommand, BaseResponse<string>>
    {
        private readonly IGenericRepository<InspectionAssignment> _assignRepo;
        private readonly IGenericRepository<InspectionReport> _reportRepo;

        public SubmitServiceCenterInspectionCommandHandler(
            IGenericRepository<InspectionAssignment> assignRepo,
            IGenericRepository<InspectionReport> reportRepo)
        {
            _assignRepo = assignRepo;
            _reportRepo = reportRepo;
        }

        public async Task<BaseResponse<string>> Handle(SubmitServiceCenterInspectionCommand cmd, CancellationToken ct)
        {
            var assignment = await _assignRepo.GetByIdAsync(cmd.AssignmentId);
            if (assignment == null) return BaseResponse<string>.Fail("Assignment not found.");

            if (assignment.AgentId != cmd.AgentId)
                return BaseResponse<string>.Fail("Not authorized to submit report for this assignment.");

            if (assignment.Status == "Completed")
                return BaseResponse<string>.Fail("Report already submitted.");

            var report = new InspectionReport
            {
                AssignmentId = assignment.Id,
                StartedAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow,
                OverallDescription = cmd.OverallDescription
            };

            await _reportRepo.AddAsync(report);

            assignment.Status = "Completed";
            await _assignRepo.UpdateAsync(assignment);

            return BaseResponse<string>.Ok(string.Empty, "Service Center inspection report submitted successfully.");
        }
    }
}

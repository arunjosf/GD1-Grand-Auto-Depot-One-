using GD1.Application.Common;
using GD1.Domain.Interfaces;
using GD1.Domain.Entities;
using GD1.Domain.Entities.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Application.Features.GD1Admin.Commands
{
    public class ReviewAppealCommand : IRequest<BaseResponse<string>>
    {
        public long RequestId { get; set; } 
        public AppealDecision Decision { get; set; } 
        public string? Reason { get; set; }
        public long AdminId { get; set; }
    }

    public class ReviewAppealCommandHandler : IRequestHandler<ReviewAppealCommand, BaseResponse<string>>
    {
        private readonly IGenericRepository<AgentRequest> _requestRepo;
        private readonly IGenericRepository<InspectionAssignment> _assignRepo;

        public ReviewAppealCommandHandler(
            IGenericRepository<AgentRequest> requestRepo,
            IGenericRepository<InspectionAssignment> assignRepo)
        {
            _requestRepo = requestRepo;
            _assignRepo = assignRepo;
        }

        public async Task<BaseResponse<string>> Handle(ReviewAppealCommand cmd, CancellationToken ct)
        {
            var request = await _requestRepo.GetByIdAsync(cmd.RequestId);
            if (request == null) return BaseResponse<string>.Fail("Request not found.");

            if (request.Status != AppealStatus.Pending)
                return BaseResponse<string>.Fail($"This request has already been processed as '{request.Status}'.");

            request.Status = cmd.Decision == AppealDecision.Approved 
                ? AppealStatus.Approved 
                : AppealStatus.Rejected;

            request.AdminRemarks = cmd.Reason; 

            // Logic is now perfectly reliable using Enums
            if (cmd.Decision == AppealDecision.Approved && request.RequestedDate.HasValue)
            {
                var assignment = await _assignRepo.GetByIdAsync(request.AssignmentId);
                if (assignment != null)
                {
                    assignment.ScheduledDate = request.RequestedDate.Value;
                    assignment.Status = "Assigned"; 
                    await _assignRepo.UpdateAsync(assignment);
                }
            }

            await _requestRepo.UpdateAsync(request);
            
            var resultMsg = cmd.Decision == AppealDecision.Approved 
                ? "Request approved and assignment updated."
                : "Request rejected.";

            return BaseResponse<string>.Ok(resultMsg);
        }
    }
}

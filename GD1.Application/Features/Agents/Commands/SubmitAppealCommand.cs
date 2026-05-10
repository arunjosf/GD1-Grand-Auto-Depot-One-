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

namespace GD1.Application.Features.Agents.Commands
{
    public class SubmitAppealCommand : IRequest<BaseResponse<string>>
    {
        public long AssignmentId { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime? RescheduleRequestDate { get; set; }
        public long UserId { get; set; } // Renamed from AgentId for clarity
    }

    public class SubmitAppealCommandHandler : IRequestHandler<SubmitAppealCommand, BaseResponse<string>>
    {
        private readonly IGenericRepository<InspectionAssignment> _assignRepo;
        private readonly IGenericRepository<AgentRequest> _requestRepo;
        private readonly IGenericRepository<Agent> _agentRepo;

        public SubmitAppealCommandHandler(
            IGenericRepository<InspectionAssignment> assignRepo,
            IGenericRepository<AgentRequest> requestRepo,
            IGenericRepository<Agent> agentRepo)
        {
            _assignRepo = assignRepo;
            _requestRepo = requestRepo;
            _agentRepo = agentRepo;
        }

        public async Task<BaseResponse<string>> Handle(SubmitAppealCommand cmd, CancellationToken ct)
        {
            // 1. Resolve UserID to AgentID
            var agents = await _agentRepo.GetAllAsync();
            var agent = agents.FirstOrDefault(a => a.UserId == cmd.UserId);
            
            if (agent == null) 
                return BaseResponse<string>.Fail("Agent profile not found for this user.");

            // 2. Fetch the assignment
            var assignment = await _assignRepo.GetByIdAsync(cmd.AssignmentId);
            if (assignment == null) return BaseResponse<string>.Fail("Assignment not found.");

            // 3. Verify the agent owns this assignment
            if (assignment.AgentId != agent.Id)
                return BaseResponse<string>.Fail("You are not authorized to submit a request for this assignment.");

            // 4. Create the request
            var agentReq = new AgentRequest
            {
                AssignmentId = assignment.Id,
                Description = cmd.Description,
                RequestedDate = cmd.RescheduleRequestDate,
                Status = AppealStatus.Pending
            };

            await _requestRepo.AddAsync(agentReq);
            return BaseResponse<string>.Ok("Request submitted to Admin successfully.");
        }
    }
}

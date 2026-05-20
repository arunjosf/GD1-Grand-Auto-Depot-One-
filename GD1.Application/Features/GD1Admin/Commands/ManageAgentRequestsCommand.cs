using GD1.Application.Common;
using GD1.Domain.Entities;
using GD1.Domain.Entities.Enums;
using GD1.Domain.Interfaces;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GD1.Application.Features.GD1Admin.DTOs;
using GD1.Application.Interfaces.Repositories;

namespace GD1.Application.Features.GD1Admin.Commands
{
    public class GetPendingAgentsQuery : IRequest<BaseResponse<List<PendingAgentDto>>> { }

    public class GetPendingAgentsQueryHandler : IRequestHandler<GetPendingAgentsQuery, BaseResponse<List<PendingAgentDto>>>
    {
        private readonly IFranchiseReadRepository _repo;
        public GetPendingAgentsQueryHandler(IFranchiseReadRepository repo) => _repo = repo;

        public async Task<BaseResponse<List<PendingAgentDto>>> Handle(GetPendingAgentsQuery request, CancellationToken ct)
        {
            var pending = (await _repo.GetPendingAgentsAsync()).ToList();
            return BaseResponse<List<PendingAgentDto>>.Ok(pending, "Retrieved pending agent requests.");
        }
    }

    // Consolidated Review Command
    public class ReviewAgentRequestCommand : IRequest<BaseResponse<bool>>
    {
        public long AgentId { get; set; }
        public AgentApprovalStatus Status { get; set; }
        public string? Reason { get; set; }
    }

    public class ReviewAgentRequestCommandHandler : IRequestHandler<ReviewAgentRequestCommand, BaseResponse<bool>>
    {
        private readonly IGenericRepository<Agent> _agentRepo;
        private readonly IGenericRepository<User> _userRepo;

        public ReviewAgentRequestCommandHandler(IGenericRepository<Agent> agentRepo, IGenericRepository<User> userRepo)
        {
            _agentRepo = agentRepo;
            _userRepo = userRepo;
        }

        public async Task<BaseResponse<bool>> Handle(ReviewAgentRequestCommand request, CancellationToken ct)
        {
            var agent = await _agentRepo.GetByIdAsync(request.AgentId);
            if (agent == null) return BaseResponse<bool>.Fail("Agent not found.");

            agent.ApprovalStatus = request.Status;

            if (request.Status == AgentApprovalStatus.Approved)
            {
                agent.IsVerified = true;
                agent.IsActive = true;

                var user = await _userRepo.GetByIdAsync(agent.Id);
                if (user != null)
                {
                    user.IsActive = true;
                    await _userRepo.UpdateAsync(user);
                }
            }
            else if (request.Status == AgentApprovalStatus.Rejected || request.Status == AgentApprovalStatus.Suspended)
            {
                agent.IsActive = false;
                
                var user = await _userRepo.GetByIdAsync(agent.Id);
                if (user != null)
                {
                    user.IsActive = false;
                    await _userRepo.UpdateAsync(user);
                }
            }

            await _agentRepo.UpdateAsync(agent);
            return BaseResponse<bool>.Ok(true, $"Agent request has been {request.Status}.");
        }
    }
}

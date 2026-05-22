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
    // ---------------------------------------------------------------------------
    // GET pending staff requests
    // GD1Admin  → pending Agents
    // LotOwner  → pending Managers on their properties
    // ---------------------------------------------------------------------------
    public class GetPendingAgentsQuery : IRequest<BaseResponse<List<PendingStaffDto>>>
    {
        /// <summary>Null when called by GD1Admin. Set to LotOwner's UserId when called by LotOwner.</summary>
        public long? LotOwnerId { get; set; }
    }

    public class GetPendingAgentsQueryHandler : IRequestHandler<GetPendingAgentsQuery, BaseResponse<List<PendingStaffDto>>>
    {
        private readonly IFranchiseReadRepository _franchiseRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.LotManager> _lotManagerRepo;
        private readonly IGenericRepository<VehicleStorageProperty> _propertyRepo;
        private readonly IGenericRepository<User> _userRepo;
        private readonly IGenericRepository<Agent> _agentRepo;

        public GetPendingAgentsQueryHandler(
            IFranchiseReadRepository franchiseRepo,
            IGenericRepository<GD1.Domain.Entities.LotManager> lotManagerRepo,
            IGenericRepository<VehicleStorageProperty> propertyRepo,
            IGenericRepository<User> userRepo,
            IGenericRepository<Agent> agentRepo)
        {
            _franchiseRepo = franchiseRepo;
            _lotManagerRepo = lotManagerRepo;
            _propertyRepo = propertyRepo;
            _userRepo = userRepo;
            _agentRepo = agentRepo;
        }

        public async Task<BaseResponse<List<PendingStaffDto>>> Handle(GetPendingAgentsQuery request, CancellationToken ct)
        {
            var result = new List<PendingStaffDto>();

            if (request.LotOwnerId.HasValue)
            {
                // LotOwner path — return pending Managers on their properties
                var ownedProps = await _propertyRepo.FindAsync(p => p.LotOwnerId == request.LotOwnerId.Value);
                var ownedPropIds = ownedProps.Select(p => p.Id).ToList();

                var pendingRecords = await _lotManagerRepo.FindAsync(
                    m => ownedPropIds.Contains(m.PropertyId) &&
                         m.ApprovalStatus == AgentApprovalStatus.Pending);

                foreach (var record in pendingRecords)
                {
                    var user = await _userRepo.GetByIdAsync(record.ManagerId);
                    if (user is null) continue;

                    var prop = ownedProps.FirstOrDefault(p => p.Id == record.PropertyId);

                    result.Add(new PendingStaffDto
                    {
                        Id = record.Id,          // LotManager record ID (used for review)
                        FullName = user.FullName,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        Role = "Manager",
                        SelfieUrl = record.SelfieUrl,
                        IdProofUrl = record.IdProofUrl,
                        PropertyId = record.PropertyId,
                        PropertyName = prop?.Name
                    });
                }
            }
            else
            {
                // GD1Admin path — return pending Agents
                var pendingAgents = await _franchiseRepo.GetPendingAgentsAsync();
                result.AddRange(pendingAgents.Select(a => new PendingStaffDto
                {
                    Id = a.Id,
                    FullName = a.FullName,
                    Email = a.Email,
                    PhoneNumber = a.PhoneNumber,
                    Role = "Agent",
                    SelfieUrl = a.SelfieUrl,
                    IdProofUrl = a.IdProofUrl,
                    City = a.City,
                    State = a.State
                }));
            }

            return BaseResponse<List<PendingStaffDto>>.Ok(result,
                $"{result.Count} pending request(s) found.");
        }
    }

    // ---------------------------------------------------------------------------
    // REVIEW (approve / reject) a pending staff request
    // GD1Admin  → reviews Agent (Id = AgentId)
    // LotOwner  → reviews Manager (Id = LotManager record Id)
    // ---------------------------------------------------------------------------
    public class ReviewAgentRequestCommand : IRequest<BaseResponse<bool>>
    {
        public long Id { get; set; }
        public AgentApprovalStatus Status { get; set; }
        public string? Reason { get; set; }

        /// <summary>Null when called by GD1Admin. Set to LotOwner's UserId when called by LotOwner.</summary>
        public long? LotOwnerId { get; set; }
    }

    public class ReviewAgentRequestCommandHandler : IRequestHandler<ReviewAgentRequestCommand, BaseResponse<bool>>
    {
        private readonly IGenericRepository<Agent> _agentRepo;
        private readonly IGenericRepository<User> _userRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.LotManager> _lotManagerRepo;
        private readonly IGenericRepository<VehicleStorageProperty> _propertyRepo;

        public ReviewAgentRequestCommandHandler(
            IGenericRepository<Agent> agentRepo,
            IGenericRepository<User> userRepo,
            IGenericRepository<GD1.Domain.Entities.LotManager> lotManagerRepo,
            IGenericRepository<VehicleStorageProperty> propertyRepo)
        {
            _agentRepo = agentRepo;
            _userRepo = userRepo;
            _lotManagerRepo = lotManagerRepo;
            _propertyRepo = propertyRepo;
        }

        public async Task<BaseResponse<bool>> Handle(ReviewAgentRequestCommand request, CancellationToken ct)
        {
            if (request.LotOwnerId.HasValue)
            {
                // LotOwner reviewing a Manager — Id = LotManager record Id
                var lotManager = await _lotManagerRepo.GetByIdAsync(request.Id);
                if (lotManager is null) return BaseResponse<bool>.Fail("Manager record not found.");

                // Verify ownership
                var prop = await _propertyRepo.GetByIdAsync(lotManager.PropertyId);
                if (prop is null || prop.LotOwnerId != request.LotOwnerId.Value)
                    return BaseResponse<bool>.Fail("You do not have permission to review this manager.");

                lotManager.ApprovalStatus = request.Status;

                var managerUser = await _userRepo.GetByIdAsync(lotManager.ManagerId);
                if (managerUser != null)
                {
                    if (request.Status == AgentApprovalStatus.Approved)
                    {
                        lotManager.IsActive = true;
                        managerUser.IsActive = true;
                    }
                    else
                    {
                        lotManager.IsActive = false;
                        managerUser.IsActive = false;
                    }
                    await _userRepo.UpdateAsync(managerUser);
                }

                await _lotManagerRepo.UpdateAsync(lotManager);
                return BaseResponse<bool>.Ok(true, $"Manager access has been {request.Status}.");
            }
            else
            {
                // GD1Admin reviewing an Agent — Id = AgentId
                var agent = await _agentRepo.GetByIdAsync(request.Id);
                if (agent == null) return BaseResponse<bool>.Fail("Agent not found.");

                agent.ApprovalStatus = request.Status;
                var user = await _userRepo.GetByIdAsync(agent.Id);

                if (request.Status == AgentApprovalStatus.Approved)
                {
                    agent.IsVerified = true;
                    agent.IsActive = true;
                    if (user != null) { user.IsActive = true; await _userRepo.UpdateAsync(user); }
                }
                else if (request.Status == AgentApprovalStatus.Rejected || request.Status == AgentApprovalStatus.Suspended)
                {
                    agent.IsActive = false;
                    if (user != null) { user.IsActive = false; await _userRepo.UpdateAsync(user); }
                }

                await _agentRepo.UpdateAsync(agent);
                return BaseResponse<bool>.Ok(true, $"Agent request has been {request.Status}.");
            }
        }
    }
}

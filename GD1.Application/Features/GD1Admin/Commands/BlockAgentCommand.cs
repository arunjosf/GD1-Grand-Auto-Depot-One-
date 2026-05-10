using GD1.Application.Common;
using GD1.Domain.Entities;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Application.Features.GD1Admin.Commands
{
    public class BlockAgentCommand : IRequest<BaseResponse<string>>
    {
        public long AgentId { get; set; }
    }

    public class BlockAgentCommandHandler
        : IRequestHandler<BlockAgentCommand, BaseResponse<string>>
    {
        private readonly IGenericRepository<Agent> _repo;

        public BlockAgentCommandHandler(IGenericRepository<Agent> repo)
            => _repo = repo;

        public async Task<BaseResponse<string>> Handle(
            BlockAgentCommand cmd, CancellationToken ct)
        {
            var agent = await _repo.GetByIdAsync(cmd.AgentId);
            if (agent is null)
                throw new KeyNotFoundException("Agent not found.");

            agent.IsActive = !agent.IsActive;
            await _repo.UpdateAsync(agent);

            var msg = agent.IsActive ? "Agent activated." : "Agent blocked.";
            return BaseResponse<string>.Ok(string.Empty, msg);
        }
    }
}

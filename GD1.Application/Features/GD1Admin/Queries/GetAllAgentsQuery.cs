using GD1.Application.Common;
using GD1.Application.Interfaces.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GD1.Application.Features.GD1Admin.DTOs;

namespace GD1.Application.Features.GD1Admin.Queries
{
    public class GetAllAgentsQuery
        : IRequest<BaseResponse<IEnumerable<AgentDto>>>
    {
        public bool OnlyActive { get; set; } = true;
        public string? City { get; set; }
        public string? State { get; set; }
    }

    public class GetAllAgentsQueryHandler
        : IRequestHandler<GetAllAgentsQuery, BaseResponse<IEnumerable<AgentDto>>>
    {
        private readonly IFranchiseReadRepository _repo;

        public GetAllAgentsQueryHandler(IFranchiseReadRepository repo)
            => _repo = repo;

        public async Task<BaseResponse<IEnumerable<AgentDto>>> Handle(
            GetAllAgentsQuery query, CancellationToken ct)
        {
            var agents = await _repo.GetAllAgentsAsync(
                query.OnlyActive, query.City, query.State);
            return BaseResponse<IEnumerable<AgentDto>>.Ok(agents);
        }
    }
}

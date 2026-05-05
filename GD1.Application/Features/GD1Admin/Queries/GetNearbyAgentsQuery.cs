using GD1.Application.Common;
using GD1.Application.Features.GD1Admin.DTOs;
using GD1.Application.Interfaces.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Application.Features.GD1Admin.Queries
{
    public class GetNearbyAgentsQuery
        : IRequest<BaseResponse<IEnumerable<AgentDto>>>
    {
        public long ApplicationId { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
    }

    public class GetNearbyAgentsQueryHandler
        : IRequestHandler<GetNearbyAgentsQuery, BaseResponse<IEnumerable<AgentDto>>>
    {
        private readonly IFranchiseReadRepository _repo;

        public GetNearbyAgentsQueryHandler(IFranchiseReadRepository repo)
            => _repo = repo;

        public async Task<BaseResponse<IEnumerable<AgentDto>>> Handle(
            GetNearbyAgentsQuery query, CancellationToken ct)
        {
            var state = query.State;
            var city = query.City;
            
            if (string.IsNullOrEmpty(state) && query.ApplicationId > 0)
            {
                var app = await _repo.GetByIdAsync(query.ApplicationId, 0); // We can just pass 0 for ApplicantId if not needed for Admin or change query
                if (app != null)
                {
                    state = app.State;
                    city = app.City;
                }
            }
            
            var agents = await _repo.GetNearbyAgentsAsync(city ?? "", state ?? "");
            return BaseResponse<IEnumerable<AgentDto>>.Ok(agents);
        }
    }
}
    

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
            if (query.ApplicationId <= 0)
                return BaseResponse<IEnumerable<AgentDto>>.Fail("Application ID is required.");

            var app = await _repo.GetByIdAsync(query.ApplicationId, 0);
            if (app == null)
                return BaseResponse<IEnumerable<AgentDto>>.Fail("Application not found.");

            // 1. Try Coordinate-based search (Most accurate)
            if (app.Latitude.HasValue && app.Longitude.HasValue && app.Latitude != 0)
            {
                var nearbyAgents = await _repo.GetNearbyAgentsAsync(app.Latitude.Value, app.Longitude.Value);
                return BaseResponse<IEnumerable<AgentDto>>.Ok(nearbyAgents);
            }

            // 2. Fallback: Search by City and State (Professional Fallback)
            var allAgents = await _repo.GetAllAgentsAsync(true, null, null);
            var cityAgents = allAgents.Where(a => 
                (a.City != null && a.City.Equals(app.City, StringComparison.OrdinalIgnoreCase)) ||
                (a.State != null && a.State.Equals(app.State, StringComparison.OrdinalIgnoreCase))
            ).ToList();

            if (cityAgents.Any())
            {
                return BaseResponse<IEnumerable<AgentDto>>.Ok(cityAgents, $"Found {cityAgents.Count} agents in {app.City}, {app.State}. (Distance unavailable due to missing coordinates)");
            }

            // 3. Final Fallback: Return all verified agents
            return BaseResponse<IEnumerable<AgentDto>>.Ok(allAgents, "No agents found in the same city. Showing all available agents.");
        }
    }
}
    

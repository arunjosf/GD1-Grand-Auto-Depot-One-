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
    public class GetNearbyAgentsQuery : IRequest<BaseResponse<IEnumerable<UserListDto>>>
    {
        public long? ApplicationId { get; set; }
    }

    public class GetNearbyAgentsQueryHandler : IRequestHandler<GetNearbyAgentsQuery, BaseResponse<IEnumerable<UserListDto>>>
    {
        private readonly IFranchiseReadRepository _repo;

        public GetNearbyAgentsQueryHandler(IFranchiseReadRepository repo)
        {
            _repo = repo;
        }

        public async Task<BaseResponse<IEnumerable<UserListDto>>> Handle(GetNearbyAgentsQuery request, CancellationToken cancellationToken)
        {
            if (!request.ApplicationId.HasValue)
                return BaseResponse<IEnumerable<UserListDto>>.Fail("Application ID is required.");

            var app = await _repo.GetByIdAsync(request.ApplicationId.Value, 0);
            if (app == null)
                return BaseResponse<IEnumerable<UserListDto>>.Fail("Application not found.");

            // 1. If application has coordinates, get geographically nearby agents
            if (app.Latitude.HasValue && app.Longitude.HasValue && (app.Latitude != 0 || app.Longitude != 0))
            {
                var nearbyAgents = await _repo.GetNearbyAgentsAsync(app.Latitude.Value, app.Longitude.Value);
                return BaseResponse<IEnumerable<UserListDto>>.Ok(nearbyAgents);
            }

            // 2. Fallback: Get agents in the same city
            var cityAgents = (await _repo.GetAllAgentsAsync(true, app.City, app.State)).ToList();
            if (cityAgents.Any())
            {
                return BaseResponse<IEnumerable<UserListDto>>.Ok(cityAgents, $"Found {cityAgents.Count} agents in {app.City}, {app.State}. (Distance unavailable due to missing coordinates)");
            }

            // 3. Absolute Fallback: Get all active agents
            var allAgents = await _repo.GetAllAgentsAsync(true, null, null);
            return BaseResponse<IEnumerable<UserListDto>>.Ok(allAgents, "No agents found in the same city. Showing all available agents.");
        }
    }
}

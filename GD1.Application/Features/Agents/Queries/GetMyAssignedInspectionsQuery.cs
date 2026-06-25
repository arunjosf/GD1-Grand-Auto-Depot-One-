using GD1.Application.Common;
using GD1.Application.Features.FranchiseApplication.DTOs;
using GD1.Application.Interfaces.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Application.Features.Agents.Queries
{
    public class GetMyAssignedInspectionsQuery : IRequest<BaseResponse<List<ApplicationDto>>>
    {
        public long AgentId { get; set; }
    }

    public class GetMyAssignedInspectionsQueryHandler : IRequestHandler<GetMyAssignedInspectionsQuery, BaseResponse<List<ApplicationDto>>>
    {
        private readonly IFranchiseReadRepository _repo;

        public GetMyAssignedInspectionsQueryHandler(IFranchiseReadRepository repo)
        {
            _repo = repo;
        }

        public async Task<BaseResponse<List<ApplicationDto>>> Handle(GetMyAssignedInspectionsQuery req, CancellationToken cancellationToken)
        {
            var apps = (await _repo.GetAgentAssignedApplicationsAsync(req.AgentId)).ToList();
            return BaseResponse<List<ApplicationDto>>.Ok(apps);
        }
    }
}

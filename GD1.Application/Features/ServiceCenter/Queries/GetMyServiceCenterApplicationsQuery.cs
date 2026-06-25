using GD1.Application.Common;
using GD1.Application.Features.ServiceCenter.DTOs;
using GD1.Application.Interfaces;
using GD1.Application.Interfaces.Repositories;
using GD1.Domain.Interfaces;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.ServiceCenter.Queries
{
    public class GetMyServiceCenterApplicationsQuery : IRequest<BaseResponse<IEnumerable<UserServiceCenterApplicationDto>>>
    {
        public long ApplicantId { get; set; }
    }

    public class GetMyServiceCenterApplicationsQueryHandler : IRequestHandler<GetMyServiceCenterApplicationsQuery, BaseResponse<IEnumerable<UserServiceCenterApplicationDto>>>
    {
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceCenterPartneringApplication> _repo;

        public GetMyServiceCenterApplicationsQueryHandler(IGenericRepository<GD1.Domain.Entities.ServiceCenterPartneringApplication> repo)
        {
            _repo = repo;
        }

        public async Task<BaseResponse<IEnumerable<UserServiceCenterApplicationDto>>> Handle(GetMyServiceCenterApplicationsQuery query, CancellationToken ct)
        {
            var apps = await _repo.FindAsync(a => a.ApplicantId == query.ApplicantId);
            
            var userApps = apps.Select(app => new UserServiceCenterApplicationDto
            {
                Id = app.Id,
                ApplicationType = "ServiceCenter",
                BusinessName = app.Name, // Map Name to BusinessName for UI compatibility
                Status = app.Status,
                AdminNotes = app.AdminNotes,
                CreatedAt = app.CreatedAt
            });

            return BaseResponse<IEnumerable<UserServiceCenterApplicationDto>>.Ok(userApps);
        }
    }
}

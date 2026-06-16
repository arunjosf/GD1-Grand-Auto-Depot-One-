using GD1.Application.Common;
using GD1.Domain.Interfaces;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.ServiceCenter.Queries
{
    public class GetServiceCenterProfileQuery : IRequest<BaseResponse<ServiceCenterProfileDto>>
    {
        public long AdminId { get; set; }
    }

    public class ServiceCenterProfileDto
    {
        public string Name { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
    }

    public class GetServiceCenterProfileQueryHandler : IRequestHandler<GetServiceCenterProfileQuery, BaseResponse<ServiceCenterProfileDto>>
    {
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceCenter> _scRepo;

        public GetServiceCenterProfileQueryHandler(IGenericRepository<GD1.Domain.Entities.ServiceCenter> scRepo)
        {
            _scRepo = scRepo;
        }

        public async Task<BaseResponse<ServiceCenterProfileDto>> Handle(GetServiceCenterProfileQuery request, CancellationToken cancellationToken)
        {
            var centers = await _scRepo.FindAsync(x => x.AdminId == request.AdminId);
            var sc = centers.FirstOrDefault();
            if (sc == null) return BaseResponse<ServiceCenterProfileDto>.Fail("Service center not found");

            return BaseResponse<ServiceCenterProfileDto>.Ok(new ServiceCenterProfileDto { Name = sc.Name, OwnerName = sc.OwnerName }, "Success");
        }
    }
}

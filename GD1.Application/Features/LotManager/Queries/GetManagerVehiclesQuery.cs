using GD1.Application.Common;
using GD1.Application.Interfaces.Repositories;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.LotManager.Queries
{
    public class GetManagerVehiclesQuery : IRequest<BaseResponse<IEnumerable<ManagerVehicleDto>>>
    {
        public long ManagerId { get; set; }
    }

    public class GetManagerVehiclesQueryHandler : IRequestHandler<GetManagerVehiclesQuery, BaseResponse<IEnumerable<ManagerVehicleDto>>>
    {
        private readonly IManagerReadRepository _repo;
        public GetManagerVehiclesQueryHandler(IManagerReadRepository repo) => _repo = repo;

        public async Task<BaseResponse<IEnumerable<ManagerVehicleDto>>> Handle(GetManagerVehiclesQuery request, CancellationToken cancellationToken)
        {
            var data = await _repo.GetVehiclesAsync(request.ManagerId);
            return BaseResponse<IEnumerable<ManagerVehicleDto>>.Ok(data);
        }
    }
}

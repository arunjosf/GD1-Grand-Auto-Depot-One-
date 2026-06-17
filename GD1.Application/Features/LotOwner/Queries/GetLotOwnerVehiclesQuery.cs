using GD1.Application.Common;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GD1.Application.Interfaces.Repositories;
using GD1.Application.Features.LotManager.Queries;

namespace GD1.Application.Features.LotOwner.Queries
{
    public class GetLotOwnerVehiclesQuery : IRequest<BaseResponse<IEnumerable<ManagerVehicleDto>>>
    {
        public long LotOwnerId { get; set; }
    }

    public class GetLotOwnerVehiclesQueryHandler : IRequestHandler<GetLotOwnerVehiclesQuery, BaseResponse<IEnumerable<ManagerVehicleDto>>>
    {
        private readonly IBookingReadRepository _repo;
        public GetLotOwnerVehiclesQueryHandler(IBookingReadRepository repo) => _repo = repo;

        public async Task<BaseResponse<IEnumerable<ManagerVehicleDto>>> Handle(GetLotOwnerVehiclesQuery request, CancellationToken cancellationToken)
        {
            var data = await _repo.GetLotOwnerVehiclesAsync(request.LotOwnerId);
            return BaseResponse<IEnumerable<ManagerVehicleDto>>.Ok(data);
        }
    }
}

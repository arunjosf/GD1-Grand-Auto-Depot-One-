using GD1.Application.Common;
using GD1.Application.Features.LotManager.Queries;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using GD1.Application.Interfaces.Repositories;

namespace GD1.Application.Features.LotOwner.Queries
{
    public class GetLotOwnerVehicleDetailQuery : IRequest<BaseResponse<ManagerVehicleDetailDto>>
    {
        public long LotOwnerId { get; set; }
        public long VehicleId { get; set; }
        public long? BookingId { get; set; }
    }

    public class GetLotOwnerVehicleDetailQueryHandler : IRequestHandler<GetLotOwnerVehicleDetailQuery, BaseResponse<ManagerVehicleDetailDto>>
    {
        private readonly IBookingReadRepository _repo;
        public GetLotOwnerVehicleDetailQueryHandler(IBookingReadRepository repo) => _repo = repo;

        public async Task<BaseResponse<ManagerVehicleDetailDto>> Handle(GetLotOwnerVehicleDetailQuery request, CancellationToken cancellationToken)
        {
            var data = await _repo.GetLotOwnerVehicleDetailAsync(request.LotOwnerId, request.VehicleId, request.BookingId);
            if (data == null) return BaseResponse<ManagerVehicleDetailDto>.Fail("Vehicle not found");

            return BaseResponse<ManagerVehicleDetailDto>.Ok(data);
        }
    }
}

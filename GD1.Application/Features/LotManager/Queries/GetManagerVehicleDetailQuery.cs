using GD1.Application.Common;
using GD1.Application.Interfaces.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.LotManager.Queries
{
    public class GetManagerVehicleDetailQuery : IRequest<BaseResponse<ManagerVehicleDetailDto>>
    {
        public long ManagerId { get; set; }
        public long VehicleId { get; set; }
        public long? BookingId { get; set; }
    }

    public class GetManagerVehicleDetailQueryHandler : IRequestHandler<GetManagerVehicleDetailQuery, BaseResponse<ManagerVehicleDetailDto>>
    {
        private readonly IManagerReadRepository _repo;
        public GetManagerVehicleDetailQueryHandler(IManagerReadRepository repo) => _repo = repo;

        public async Task<BaseResponse<ManagerVehicleDetailDto>> Handle(GetManagerVehicleDetailQuery request, CancellationToken cancellationToken)
        {
            var data = await _repo.GetVehicleDetailAsync(request.ManagerId, request.VehicleId, request.BookingId);
            if (data == null)
            {
                return BaseResponse<ManagerVehicleDetailDto>.Fail("Vehicle not found or you don't have access.");
            }
            return BaseResponse<ManagerVehicleDetailDto>.Ok(data);
        }
    }
}

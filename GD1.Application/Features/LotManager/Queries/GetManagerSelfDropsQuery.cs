using GD1.Application.Common;
using GD1.Application.Interfaces.Repositories;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.LotManager.Queries
{
    public class GetManagerSelfDropsQuery : IRequest<BaseResponse<IEnumerable<SelfDropDto>>>
    {
        public long ManagerId { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class SelfDropDto
    {
        public long BookingId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string VehicleBrand { get; set; } = string.Empty;
        public string VehicleModel { get; set; } = string.Empty;
        public string RegistrationNo { get; set; } = string.Empty;
        public System.DateTime StartDate { get; set; }
        public System.DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string VehicleImage { get; set; } = string.Empty;
    }

    public class GetManagerSelfDropsQueryHandler : IRequestHandler<GetManagerSelfDropsQuery, BaseResponse<IEnumerable<SelfDropDto>>>
    {
        private readonly IManagerReadRepository _repo;
        public GetManagerSelfDropsQueryHandler(IManagerReadRepository repo) => _repo = repo;

        public async Task<BaseResponse<IEnumerable<SelfDropDto>>> Handle(GetManagerSelfDropsQuery request, CancellationToken cancellationToken)
        {
            var data = await _repo.GetSelfDropsAsync(request.ManagerId, request.IsCompleted);
            return BaseResponse<IEnumerable<SelfDropDto>>.Ok(data);
        }
    }
}

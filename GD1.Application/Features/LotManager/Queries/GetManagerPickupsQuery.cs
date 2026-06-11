using GD1.Application.Common;
using GD1.Application.Interfaces.Repositories;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.LotManager.Queries
{
    public class GetManagerPickupsQuery : IRequest<BaseResponse<IEnumerable<ManagerPickupDto>>>
    {
        public long ManagerId { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class GetManagerPickupsQueryHandler : IRequestHandler<GetManagerPickupsQuery, BaseResponse<IEnumerable<ManagerPickupDto>>>
    {
        private readonly IManagerReadRepository _repo;
        public GetManagerPickupsQueryHandler(IManagerReadRepository repo) => _repo = repo;

        public async Task<BaseResponse<IEnumerable<ManagerPickupDto>>> Handle(GetManagerPickupsQuery request, CancellationToken cancellationToken)
        {
            var data = await _repo.GetPickupsAsync(request.ManagerId, request.IsCompleted);
            return BaseResponse<IEnumerable<ManagerPickupDto>>.Ok(data);
        }
    }
}

using GD1.Application.Common;
using GD1.Application.Interfaces.Repositories;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.Pickup.Queries
{
    public class GetLotOwnerPickupsQuery : IRequest<BaseResponse<IEnumerable<PickupRequestDto>>>
    {
        public long LotOwnerId { get; set; }
    }

    public class GetLotOwnerPickupsQueryHandler : IRequestHandler<GetLotOwnerPickupsQuery, BaseResponse<IEnumerable<PickupRequestDto>>>
    {
        private readonly IPickupReadRepository _repo;

        public GetLotOwnerPickupsQueryHandler(IPickupReadRepository repo)
        {
            _repo = repo;
        }

        public async Task<BaseResponse<IEnumerable<PickupRequestDto>>> Handle(
            GetLotOwnerPickupsQuery request, CancellationToken cancellationToken)
        {
            var result = await _repo.GetLotOwnerPickupsAsync(request.LotOwnerId);
            return BaseResponse<IEnumerable<PickupRequestDto>>.Ok(result);
        }
    }
}

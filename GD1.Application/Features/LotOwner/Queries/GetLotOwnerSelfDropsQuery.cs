using GD1.Application.Common;
using GD1.Application.Features.LotManager.Queries;
using GD1.Application.Interfaces.Repositories;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.LotOwner.Queries
{
    public class GetLotOwnerSelfDropsQuery : IRequest<BaseResponse<IEnumerable<SelfDropDto>>>
    {
        public long LotOwnerId { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class GetLotOwnerSelfDropsQueryHandler : IRequestHandler<GetLotOwnerSelfDropsQuery, BaseResponse<IEnumerable<SelfDropDto>>>
    {
        private readonly IBookingReadRepository _repo;
        public GetLotOwnerSelfDropsQueryHandler(IBookingReadRepository repo) => _repo = repo;

        public async Task<BaseResponse<IEnumerable<SelfDropDto>>> Handle(GetLotOwnerSelfDropsQuery request, CancellationToken cancellationToken)
        {
            var data = await _repo.GetLotOwnerSelfDropsAsync(request.LotOwnerId, request.IsCompleted);
            return BaseResponse<IEnumerable<SelfDropDto>>.Ok(data);
        }
    }
}

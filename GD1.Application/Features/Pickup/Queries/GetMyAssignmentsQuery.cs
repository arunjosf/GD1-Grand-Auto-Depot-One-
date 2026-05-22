using GD1.Application.Common;
using GD1.Application.Interfaces.Repositories;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.Pickup.Queries
{
    public class GetMyAssignmentsQuery : IRequest<BaseResponse<IEnumerable<PickupRequestDto>>>
    {
        public long ManagerUserId { get; set; }
    }

    public class GetMyAssignmentsQueryHandler : IRequestHandler<GetMyAssignmentsQuery, BaseResponse<IEnumerable<PickupRequestDto>>>
    {
        private readonly IPickupReadRepository _repo;

        public GetMyAssignmentsQueryHandler(IPickupReadRepository repo)
        {
            _repo = repo;
        }

        public async Task<BaseResponse<IEnumerable<PickupRequestDto>>> Handle(
            GetMyAssignmentsQuery request, CancellationToken cancellationToken)
        {
            var result = await _repo.GetMyAssignmentsAsync(request.ManagerUserId);
            return BaseResponse<IEnumerable<PickupRequestDto>>.Ok(result);
        }
    }
}

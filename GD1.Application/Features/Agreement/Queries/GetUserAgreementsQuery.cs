using GD1.Application.Common;
using GD1.Domain.Entities;
using GD1.Domain.Interfaces;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.AgreementFeature.Queries
{
    public class GetUserAgreementsQuery : IRequest<BaseResponse<List<AgreementResponseDto>>>
    {
        public long UserId { get; set; }
    }

    public class GetUserAgreementsQueryHandler : IRequestHandler<GetUserAgreementsQuery, BaseResponse<List<AgreementResponseDto>>>
    {
        private readonly IGenericRepository<Agreement> _repo;

        public GetUserAgreementsQueryHandler(IGenericRepository<Agreement> repo)
        {
            _repo = repo;
        }

        public async Task<BaseResponse<List<AgreementResponseDto>>> Handle(GetUserAgreementsQuery request, CancellationToken cancellationToken)
        {
            var allAgreements = await _repo.GetAllAsync();
            var userAgreements = allAgreements.Where(a => a.UserId == request.UserId)
                                              .OrderByDescending(a => a.CreatedAt)
                                              .Select(a => new AgreementResponseDto
                                              {
                                                  Id = a.Id,
                                                  Content = a.Content,
                                                  Status = a.Status.ToString(),
                                                  Description = a.Type == GD1.Domain.Entities.Enums.AgreementType.LotBooking ? $"Agreement for Lot Booking #{a.ReferenceId}" :
                                                                a.Type == GD1.Domain.Entities.Enums.AgreementType.FranchiseApplication ? $"Agreement for Franchise Application #{a.ReferenceId}" :
                                                                $"Agreement #{a.Id}",
                                                  CreatedAt = a.CreatedAt,
                                                  AcceptedAt = a.AcceptedAt
                                              }).ToList();

            return BaseResponse<List<AgreementResponseDto>>.Ok(userAgreements);
        }
    }
}

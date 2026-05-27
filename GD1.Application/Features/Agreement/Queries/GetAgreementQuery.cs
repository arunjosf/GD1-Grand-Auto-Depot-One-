using GD1.Application.Common;
using GD1.Domain.Entities;
using GD1.Domain.Entities.Enums;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.AgreementFeature.Queries
{
    public class AgreementResponseDto
    {
        public long Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? AcceptedAt { get; set; }
    }

    public class GetAgreementQuery : IRequest<BaseResponse<AgreementResponseDto>>
    {
        public long AgreementId { get; set; }
        public long UserId { get; set; }
    }

    public class GetAgreementQueryHandler : IRequestHandler<GetAgreementQuery, BaseResponse<AgreementResponseDto>>
    {
        private readonly IGenericRepository<Agreement> _repo;

        public GetAgreementQueryHandler(IGenericRepository<Agreement> repo)
        {
            _repo = repo;
        }

        public async Task<BaseResponse<AgreementResponseDto>> Handle(GetAgreementQuery request, CancellationToken cancellationToken)
        {
            var agreement = await _repo.GetByIdAsync(request.AgreementId);
            if (agreement == null)
                return BaseResponse<AgreementResponseDto>.Fail("Agreement not found.");

            if (agreement.UserId != request.UserId)
                return BaseResponse<AgreementResponseDto>.Fail("Unauthorized.");

            return BaseResponse<AgreementResponseDto>.Ok(new AgreementResponseDto
            {
                Id = agreement.Id,
                Content = agreement.Content,
                Status = agreement.Status.ToString(),
                Description = agreement.Type switch
                {
                    AgreementType.LotBooking => $"Agreement for Lot Booking #{agreement.ReferenceId}",
                    AgreementType.FranchiseApplication => $"Agreement for Franchise Application #{agreement.ReferenceId}",
                    _ => $"Agreement #{agreement.Id}"
                },
                CreatedAt = agreement.CreatedAt,
                AcceptedAt = agreement.AcceptedAt
            });
        }
    }
}

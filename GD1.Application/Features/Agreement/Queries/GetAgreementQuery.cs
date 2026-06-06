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
        public int Type { get; set; }
        public long ReferenceId { get; set; }
    }

    public class GetAgreementQuery : IRequest<BaseResponse<AgreementResponseDto>>
    {
        public long? AgreementId { get; set; }
        public long? ReferenceId { get; set; }
        public AgreementType? Type { get; set; }
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
            Agreement? agreement = null;
            if (request.AgreementId.HasValue && request.AgreementId.Value > 0)
            {
                agreement = await _repo.GetByIdAsync(request.AgreementId.Value);
                
                // Fallback for cases where BookingId was passed instead of AgreementId
                if (agreement == null)
                {
                    var agreements = await _repo.FindAsync(a => a.ReferenceId == request.AgreementId.Value && a.Type == AgreementType.LotBooking);
                    agreement = System.Linq.Enumerable.FirstOrDefault(agreements);
                }
            }
            else if (request.ReferenceId.HasValue && request.Type.HasValue)
            {
                var agreements = await _repo.FindAsync(a => a.ReferenceId == request.ReferenceId.Value && a.Type == request.Type.Value);
                agreement = System.Linq.Enumerable.FirstOrDefault(agreements);
            }

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
                AcceptedAt = agreement.AcceptedAt,
                Type = (int)agreement.Type,
                ReferenceId = agreement.ReferenceId
            });
        }
    }
}

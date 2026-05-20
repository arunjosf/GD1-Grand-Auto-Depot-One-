using GD1.Application.Common;
using GD1.Domain.Entities;
using GD1.Domain.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.LotBooking.Queries
{
    public class GetAgreementQuery : IRequest<BaseResponse<string>>
    {
        public long BookingId { get; set; }
        public long OwnerId { get; set; }
    }

    public class GetAgreementQueryHandler : IRequestHandler<GetAgreementQuery, BaseResponse<string>>
    {
        private readonly IGenericRepository<BookingAgreement> _repo;

        public GetAgreementQueryHandler(IGenericRepository<BookingAgreement> repo)
        {
            _repo = repo;
        }

        public async Task<BaseResponse<string>> Handle(GetAgreementQuery request, CancellationToken cancellationToken)
        {
            var agreements = await _repo.FindAsync(a => a.BookingId == request.BookingId);
            var agreement = System.Linq.Enumerable.FirstOrDefault(agreements);
            
            if (agreement == null)
                return BaseResponse<string>.Fail("Agreement not found.");
                
            if (agreement.OwnerId != request.OwnerId)
                return BaseResponse<string>.Fail("Unauthorized access to this agreement.");

            return BaseResponse<string>.Ok(agreement.Content, "Agreement retrieved successfully.");
        }
    }
}

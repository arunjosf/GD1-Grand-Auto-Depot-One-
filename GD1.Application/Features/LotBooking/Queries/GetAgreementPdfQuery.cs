using GD1.Application.Common;
using GD1.Application.Interfaces.Services;
using GD1.Domain.Entities;
using GD1.Domain.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.LotBooking.Queries
{
    public class GetAgreementPdfQuery : IRequest<BaseResponse<byte[]>>
    {
        public long BookingId { get; set; }
        public long RequesterId { get; set; }
    }

    public class GetAgreementPdfQueryHandler : IRequestHandler<GetAgreementPdfQuery, BaseResponse<byte[]>>
    {
        private readonly IGenericRepository<BookingAgreement> _agreementRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.Booking> _bookingRepo;
        private readonly IPdfGeneratorService _pdfService;

        public GetAgreementPdfQueryHandler(
            IGenericRepository<BookingAgreement> agreementRepo,
            IGenericRepository<GD1.Domain.Entities.Booking> bookingRepo,
            IPdfGeneratorService pdfService)
        {
            _agreementRepo = agreementRepo;
            _bookingRepo = bookingRepo;
            _pdfService = pdfService;
        }

        public async Task<BaseResponse<byte[]>> Handle(GetAgreementPdfQuery request, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepo.GetByIdAsync(request.BookingId);
            if (booking == null) return BaseResponse<byte[]>.Fail("Booking not found.");

            if (booking.OwnerId != request.RequesterId)
                return BaseResponse<byte[]>.Fail("You are not authorized to access this agreement.");

            var agreements = await _agreementRepo.FindAsync(a => a.BookingId == request.BookingId);
            var agreement = System.Linq.Enumerable.FirstOrDefault(agreements);

            if (agreement == null)
                return BaseResponse<byte[]>.Fail("Agreement not found.");

            if (string.IsNullOrEmpty(agreement.PdfUrl))
                return BaseResponse<byte[]>.Fail("PDF is still generating in the background. Please try again in 5 seconds.");

            using var httpClient = new System.Net.Http.HttpClient();
            var pdfBytes = await httpClient.GetByteArrayAsync(agreement.PdfUrl);

            return BaseResponse<byte[]>.Ok(pdfBytes, "PDF successfully fetched from cloud.");
        }
    }
}

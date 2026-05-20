using GD1.Application.Common;
using GD1.Domain.Entities;
using GD1.Domain.Entities.Enums;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.LotBooking.Commands
{
    public class RespondAgreementCommand : IRequest<BaseResponse<string>>
    {
        public long BookingId { get; set; }
        public AgreementStatus Response { get; set; }
        public long OwnerId { get; set; }
        public string? IpAddress { get; set; }
    }

    public class RespondAgreementCommandHandler : IRequestHandler<RespondAgreementCommand, BaseResponse<string>>
    {
        private readonly IGenericRepository<BookingAgreement> _agreementRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.Booking> _bookingRepo;

        public RespondAgreementCommandHandler(
            IGenericRepository<BookingAgreement> agreementRepo,
            IGenericRepository<GD1.Domain.Entities.Booking> bookingRepo)
        {
            _agreementRepo = agreementRepo;
            _bookingRepo = bookingRepo;
        }

        public async Task<BaseResponse<string>> Handle(RespondAgreementCommand cmd, CancellationToken ct)
        {
            var booking = await _bookingRepo.GetByIdAsync(cmd.BookingId);
            if (booking == null) return BaseResponse<string>.Fail("Booking not found.");

            var agreements = await _agreementRepo.FindAsync(a => a.BookingId == cmd.BookingId);
            var agreement = System.Linq.Enumerable.FirstOrDefault(agreements);

            if (agreement == null) return BaseResponse<string>.Fail("Agreement not found.");

            if (agreement.OwnerId != cmd.OwnerId)
                return BaseResponse<string>.Fail("You are not authorized to respond to this agreement.");

            if (agreement.Status != AgreementStatus.Pending)
                return BaseResponse<string>.Fail("This agreement has already been responded to.");

            if (cmd.Response == AgreementStatus.Pending)
                return BaseResponse<string>.Fail("Invalid response.");

            if (cmd.Response == AgreementStatus.Accepted)
            {
                agreement.Status = cmd.Response;
                agreement.SignedAt = DateTime.UtcNow;
                agreement.IpAddress = cmd.IpAddress;
                await _agreementRepo.UpdateAsync(agreement);

                booking.Status = BookingStatus.Confirmed;
                booking.IsAgreementSigned = true;
                await _bookingRepo.UpdateAsync(booking);

                return BaseResponse<string>.Ok(string.Empty, "Agreement accepted successfully. Your booking is now confirmed.");
            }
            else
            {
                // Rejected - clean up the temporary booking
                await _bookingRepo.DeleteAsync(booking);
                return BaseResponse<string>.Ok(string.Empty, "Agreement rejected. The temporary booking has been cancelled.");
            }
        }
    }
}

using GD1.Application.Common;
using GD1.Domain.Entities;
using GD1.Domain.Entities.Enums;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.AgreementFeature.Commands
{
    public class RespondAgreementCommand : IRequest<BaseResponse<string>>
    {
        public long AgreementId { get; set; }
        public AgreementResponse Response { get; set; }
        public string? RejectionReason { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public long UserId { get; set; }
    }

    public class RespondAgreementCommandHandler : IRequestHandler<RespondAgreementCommand, BaseResponse<string>>
    {
        private readonly IGenericRepository<Agreement> _agreementRepo;
        private readonly IGenericRepository<Booking> _bookingRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.FranchiseApplication> _franchiseRepo;
        private readonly IGenericRepository<Notification> _notificationRepo;
        public RespondAgreementCommandHandler(
            IGenericRepository<Agreement> agreementRepo,
            IGenericRepository<Booking> bookingRepo,
            IGenericRepository<GD1.Domain.Entities.FranchiseApplication> franchiseRepo,
            IGenericRepository<Notification> notificationRepo)
        {
            _agreementRepo = agreementRepo;
            _bookingRepo = bookingRepo;
            _franchiseRepo = franchiseRepo;
            _notificationRepo = notificationRepo;
        }

        public async Task<BaseResponse<string>> Handle(RespondAgreementCommand request, CancellationToken cancellationToken)
        {
            Agreement? agreement = null;
            if (request.AgreementId > 0)
            {
                agreement = await _agreementRepo.GetByIdAsync(request.AgreementId);

                // Fallback for cases where BookingId was passed instead of AgreementId
                if (agreement == null)
                {
                    var agreements = await _agreementRepo.FindAsync(a => a.ReferenceId == request.AgreementId && a.Type == AgreementType.LotBooking);
                    agreement = System.Linq.Enumerable.FirstOrDefault(agreements);
                }
            }

            if (agreement == null)
                return BaseResponse<string>.Fail("Agreement not found.");

            if (agreement.UserId != request.UserId)
                return BaseResponse<string>.Fail("Unauthorized.");

            // Check removed to allow re-responding to the agreement.

            agreement.Status = request.Response == AgreementResponse.Approve ? AgreementStatus.Accepted : AgreementStatus.Rejected;
            agreement.AcceptedAt = request.Response == AgreementResponse.Approve ? DateTime.UtcNow : null;
            await _agreementRepo.UpdateAsync(agreement);

            // Handle side-effects based on AgreementType
            bool isAccepted = request.Response == AgreementResponse.Approve;
            switch (agreement.Type)
            {
                case AgreementType.LotBooking:
                    return await HandleLotBookingResponse(agreement, isAccepted, request.RejectionReason);


                case AgreementType.FranchiseApplication:
                    return await HandleFranchiseResponse(agreement, isAccepted, request.RejectionReason);

                default:
                    return BaseResponse<string>.Ok($"Agreement {agreement.Status}. No automatic side-effects configured for {agreement.Type}.");
            }
        }

        private async Task<BaseResponse<string>> HandleLotBookingResponse(Agreement agreement, bool isAccepted, string? rejectionReason)
        {
            var booking = await _bookingRepo.GetByIdAsync(agreement.ReferenceId);
            if (booking == null) return BaseResponse<string>.Fail("Booking not found.");

            // Remove related notifications for the vehicle owner
            var notifs = await _notificationRepo.FindAsync(n => n.UserId == booking.OwnerId && (n.ActionUrl == $"/agreement/{booking.Id}" || n.ActionUrl == "/user/bookings"));
            foreach (var n in notifs) { await _notificationRepo.DeleteAsync(n); }

            if (isAccepted)
            {
                booking.Status = GD1.Domain.Entities.Enums.BookingStatus.Confirmed;
                booking.IsAgreementSigned = 1;
                await _bookingRepo.UpdateAsync(booking);
                return BaseResponse<string>.Ok("Agreement Accepted. Booking has been confirmed.");
            }
            else
            {
                booking.Status = BookingStatus.AgreementDeclined;
                booking.IsAgreementSigned = 2;
                booking.RejectionReason = rejectionReason;
                await _bookingRepo.UpdateAsync(booking);
                return BaseResponse<string>.Ok("Agreement Rejected. The booking has been declined with reason.");
            }
        }


        private async Task<BaseResponse<string>> HandleFranchiseResponse(Agreement agreement, bool isAccepted, string? rejectionReason)
        {
            var app = await _franchiseRepo.GetByIdAsync(agreement.ReferenceId);
            if (app == null) return BaseResponse<string>.Fail("Franchise Application not found.");

            if (isAccepted)
            {
                app.Status = GD1.Domain.Entities.Enums.FranchiseStatus.Submitted;
                await _franchiseRepo.UpdateAsync(app);
                return BaseResponse<string>.Ok("Agreement Accepted. Franchise Application submitted to admin for review.");
            }
            else
            {
                app.Status = GD1.Domain.Entities.Enums.FranchiseStatus.Rejected;
                app.RejectionReason = rejectionReason;
                await _franchiseRepo.UpdateAsync(app);
                return BaseResponse<string>.Ok("Agreement Rejected. The franchise application has been marked as rejected.");
            }
        }
    }
}

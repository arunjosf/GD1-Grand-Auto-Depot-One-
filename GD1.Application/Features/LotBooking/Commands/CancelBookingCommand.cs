using GD1.Application.Common;
using GD1.Application.Interfaces;
using GD1.Application.Interfaces.Services;
using GD1.Domain.Entities;
using GD1.Domain.Entities.Enums;
using GD1.Domain.Interfaces;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.LotBooking.Commands
{
    public class CancelBookingCommand : IRequest<BaseResponse<string>>
    {
        public long BookingId { get; set; }
        public long OwnerId { get; set; }
        public string? Reason { get; set; }
    }

    public class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand, BaseResponse<string>>
    {
        private readonly IGenericRepository<Booking> _bookingRepo;
        private readonly IGenericRepository<VehicleStorageProperty> _propertyRepo;
        private readonly IGenericRepository<User> _userRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.LotManager> _managerRepo;
        private readonly IGenericRepository<Agreement> _agreementRepo;
        private readonly IGenericRepository<PickupRequest> _pickupRepo;
        private readonly IGenericRepository<Notification> _notificationRepo;
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;

        public CancelBookingCommandHandler(
            IGenericRepository<Booking> bookingRepo,
            IGenericRepository<VehicleStorageProperty> propertyRepo,
            IGenericRepository<User> userRepo,
            IGenericRepository<GD1.Domain.Entities.LotManager> managerRepo,
            IGenericRepository<Agreement> agreementRepo,
            IGenericRepository<PickupRequest> pickupRepo,
            IGenericRepository<Notification> notificationRepo,
            IEmailService emailService,
            INotificationService notificationService)
        {
            _bookingRepo = bookingRepo;
            _propertyRepo = propertyRepo;
            _userRepo = userRepo;
            _managerRepo = managerRepo;
            _agreementRepo = agreementRepo;
            _pickupRepo = pickupRepo;
            _notificationRepo = notificationRepo;
            _emailService = emailService;
            _notificationService = notificationService;
        }

        public async Task<BaseResponse<string>> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
        {
            var booking = (await _bookingRepo.FindAsync(b => b.Id == request.BookingId, "Vehicle")).FirstOrDefault();
            if (booking == null) return BaseResponse<string>.Fail("Booking not found.");

            if (booking.OwnerId != request.OwnerId)
                return BaseResponse<string>.Fail("You are not authorized to cancel this booking.");

            var pickupRequest = (await _pickupRepo.FindAsync(p => p.BookingId == booking.Id)).FirstOrDefault();

            if (booking.Status == BookingStatus.InLot || booking.Status == BookingStatus.Completed)
                return BaseResponse<string>.Fail("This booking cannot be cancelled from its current status. If you wish to end storage early, use the Stop Storing feature.");

            bool isChargeable = pickupRequest != null && pickupRequest.Status >= PickupStatus.InTransit;

            // Remove related notifications
            var notifs = await _notificationRepo.FindAsync(n => n.UserId == booking.OwnerId && (n.ActionUrl == $"/agreement/{booking.Id}" || n.ActionUrl == "/user/bookings"));
            foreach (var n in notifs) { await _notificationRepo.DeleteAsync(n); }

            if (isChargeable)
            {
                // Manager has picked up the vehicle and started the ride. Keep the booking in DB but mark as Cancelled.
                booking.Status = BookingStatus.Cancelled;
                booking.RejectionReason = request.Reason ?? "Cancelled after pickup request was initiated.";
                await _bookingRepo.UpdateAsync(booking);
            }
            else
            {
                // Free cancellation. Mark as Cancelled instead of deleting to keep history.
                if (pickupRequest != null)
                {
                    await _pickupRepo.DeleteAsync(pickupRequest);
                }

                booking.Status = BookingStatus.Cancelled;
                booking.RejectionReason = request.Reason ?? "Cancelled by user before any chargeable actions.";
                await _bookingRepo.UpdateAsync(booking);
            }

            // Notify Manager/Owner
            var property = await _propertyRepo.GetByIdAsync(booking.PropertyId);
            if (property != null)
            {
                var owner = await _userRepo.GetByIdAsync(property.LotOwnerId);
                if (owner != null)
                {
                    string subject = $"Booking Cancelled - {booking.Vehicle?.RegistrationNo ?? "Vehicle"}";
                    string body = $"<p>Hello,</p><p>A booking for {booking.Vehicle?.Brand} {booking.Vehicle?.Model} ({booking.Vehicle?.RegistrationNo}) at your lot ({property.Name}) has been cancelled by the vehicle owner.</p>";
                    await _emailService.SendAsync(owner.Email, subject, body);
                }

                var managers = await _managerRepo.FindAsync(m => m.PropertyId == property.Id && m.IsActive, "Manager");
                foreach (var m in managers.Where(m => m.Manager != null))
                {
                    string subject = $"Booking Cancelled - {booking.Vehicle?.RegistrationNo ?? "Vehicle"}";
                    string body = $"<p>Hello,</p><p>A booking for {booking.Vehicle?.Brand} {booking.Vehicle?.Model} ({booking.Vehicle?.RegistrationNo}) at lot {property.Name} has been cancelled by the vehicle owner.</p>";
                    await _emailService.SendAsync(m.Manager.Email, subject, body);
                }

                // Push real-time notifications
                try
                {
                    await _notificationService.SendAsync(
                        userId: property.LotOwnerId,
                        title: "Booking Cancelled",
                        body: $"The booking for {booking.Vehicle?.Brand} {booking.Vehicle?.Model} was cancelled.",
                        actionType: "ViewBooking",
                        referenceId: booking.Id);

                    if (managers.Any())
                    {
                        await _notificationService.SendToManyAsync(
                            userIds: managers.Where(m => m.ManagerId > 0).Select(m => m.ManagerId),
                            title: "Booking Cancelled",
                            body: $"The booking for {booking.Vehicle?.Brand} {booking.Vehicle?.Model} was cancelled.",
                            actionType: "ViewBooking",
                            referenceId: booking.Id);
                    }
                }
                catch { /* Ignore notification failure */ }
            }

            string msg = isChargeable ? "Booking cancelled successfully. A cancellation charge will be applied." : "Booking cancelled successfully. No charges applied.";
            return BaseResponse<string>.Ok(string.Empty, msg);
        }
    }
}

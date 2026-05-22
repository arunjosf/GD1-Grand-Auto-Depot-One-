using GD1.Application.Common;
using GD1.Application.Interfaces;
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
    }

    public class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand, BaseResponse<string>>
    {
        private readonly IGenericRepository<Booking> _bookingRepo;
        private readonly IGenericRepository<VehicleStorageProperty> _propertyRepo;
        private readonly IGenericRepository<User> _userRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.LotManager> _managerRepo;
        private readonly IEmailService _emailService;

        public CancelBookingCommandHandler(
            IGenericRepository<Booking> bookingRepo,
            IGenericRepository<VehicleStorageProperty> propertyRepo,
            IGenericRepository<User> userRepo,
            IGenericRepository<GD1.Domain.Entities.LotManager> managerRepo,
            IEmailService emailService)
        {
            _bookingRepo = bookingRepo;
            _propertyRepo = propertyRepo;
            _userRepo = userRepo;
            _managerRepo = managerRepo;
            _emailService = emailService;
        }

        public async Task<BaseResponse<string>> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
        {
            var booking = (await _bookingRepo.FindAsync(b => b.Id == request.BookingId, "Vehicle")).FirstOrDefault();
            if (booking == null) return BaseResponse<string>.Fail("Booking not found.");

            if (booking.OwnerId != request.OwnerId)
                return BaseResponse<string>.Fail("You are not authorized to cancel this booking.");

            if (booking.Status == BookingStatus.InLot || booking.Status == BookingStatus.Completed || booking.Status == BookingStatus.Cancelled)
                return BaseResponse<string>.Fail("This booking cannot be cancelled from its current status. If you wish to end storage early, use the Stop Storing feature.");

            booking.Status = BookingStatus.Cancelled;
            await _bookingRepo.UpdateAsync(booking);

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
            }

            return BaseResponse<string>.Ok(string.Empty, "Booking cancelled successfully.");
        }
    }
}

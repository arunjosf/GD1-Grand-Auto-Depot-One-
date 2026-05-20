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
    public class StopStoringCommand : IRequest<BaseResponse<string>>
    {
        public long BookingId { get; set; }
        public long OwnerId { get; set; }
    }

    public class StopStoringCommandHandler : IRequestHandler<StopStoringCommand, BaseResponse<string>>
    {
        private readonly IGenericRepository<Booking> _bookingRepo;

        public StopStoringCommandHandler(IGenericRepository<Booking> bookingRepo)
        {
            _bookingRepo = bookingRepo;
        }

        public async Task<BaseResponse<string>> Handle(StopStoringCommand request, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepo.GetByIdAsync(request.BookingId);
            
            if (booking == null) return BaseResponse<string>.Fail("Booking not found.");
            
            if (booking.OwnerId != request.OwnerId)
                return BaseResponse<string>.Fail("You are not authorized to stop this booking.");

            if (booking.Status != BookingStatus.InLot)
                return BaseResponse<string>.Fail("This feature is only available for vehicles currently stored in the lot.");

            var today = DateTime.UtcNow.Date;

            if (today >= booking.EndDate.Date)
                return BaseResponse<string>.Fail("This booking has already reached its end date.");

            int remainingDays = (booking.EndDate.Date - today).Days;

            if (remainingDays > 0)
            {
                // Refund 50% of the remaining days cost
                decimal refundAmount = (remainingDays * booking.PricePerDay) * 0.5m;
                booking.TotalCost -= refundAmount;
            }

            booking.EndDate = today;
            booking.Status = BookingStatus.Completed;
            
            await _bookingRepo.UpdateAsync(booking);

            return BaseResponse<string>.Ok(string.Empty, "Vehicle storage stopped. Partial refund calculation applied.");
        }
    }
}

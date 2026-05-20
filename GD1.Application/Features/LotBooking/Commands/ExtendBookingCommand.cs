using GD1.Application.Common;
using GD1.Domain.Entities;
using GD1.Domain.Entities.Enums;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;

namespace GD1.Application.Features.LotBooking.Commands
{
    public class ExtendBookingCommand : IRequest<BaseResponse<string>>
    {
        public long BookingId { get; set; }
        public long OwnerId { get; set; }
        public DateTime NewEndDate { get; set; }
    }

    public class ExtendBookingCommandValidator : AbstractValidator<ExtendBookingCommand>
    {
        public ExtendBookingCommandValidator()
        {
            RuleFor(x => x.BookingId).GreaterThan(0);
            RuleFor(x => x.OwnerId).GreaterThan(0);
            RuleFor(x => x.NewEndDate).NotEmpty();
        }
    }

    public class ExtendBookingCommandHandler : IRequestHandler<ExtendBookingCommand, BaseResponse<string>>
    {
        private readonly IGenericRepository<Booking> _bookingRepo;

        public ExtendBookingCommandHandler(IGenericRepository<Booking> bookingRepo)
        {
            _bookingRepo = bookingRepo;
        }

        public async Task<BaseResponse<string>> Handle(ExtendBookingCommand request, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepo.GetByIdAsync(request.BookingId);
            
            if (booking == null) return BaseResponse<string>.Fail("Booking not found.");
            
            if (booking.OwnerId != request.OwnerId)
                return BaseResponse<string>.Fail("You are not authorized to extend this booking.");

            if (booking.Status != BookingStatus.Confirmed && booking.Status != BookingStatus.InLot)
                return BaseResponse<string>.Fail("Only confirmed or active bookings can be extended.");

            if (request.NewEndDate.Date <= booking.EndDate.Date)
                return BaseResponse<string>.Fail("New end date must be after the current end date.");

            int extraDays = (request.NewEndDate.Date - booking.EndDate.Date).Days;
            decimal extraCost = extraDays * booking.PricePerDay;

            booking.EndDate = request.NewEndDate.Date;
            booking.TotalCost += extraCost;
            
            await _bookingRepo.UpdateAsync(booking);

            return BaseResponse<string>.Ok(string.Empty, $"Booking extended successfully. Additional cost: {extraCost:C}");
        }
    }
}

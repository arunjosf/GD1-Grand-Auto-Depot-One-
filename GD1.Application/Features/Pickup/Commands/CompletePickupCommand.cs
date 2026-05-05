using GD1.Domain.Entities;
using GD1.Domain.Entities.Enums;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FluentValidation;

namespace GD1.Application.Features.Pickup.Commands
{
    public record CompletePickupCommand(long PickupRequestId) : IRequest<string>;

    public class CompletePickupCommandValidator : AbstractValidator<CompletePickupCommand>
    {
        public CompletePickupCommandValidator()
        {
            RuleFor(x => x.PickupRequestId).GreaterThan(0);
        }
    }

    public class CompletePickupCommandHandler
        : IRequestHandler<CompletePickupCommand, string>
    {
        private readonly IGenericRepository<PickupRequest> _pickupRepo;
        private readonly IGenericRepository<Booking> _bookingRepo;

        public CompletePickupCommandHandler(
            IGenericRepository<PickupRequest> pickupRepo,
            IGenericRepository<Booking> bookingRepo)
        {
            _pickupRepo = pickupRepo;
            _bookingRepo = bookingRepo;
        }

        public async Task<string> Handle(
            CompletePickupCommand request,
            CancellationToken cancellationToken)
        {
            var pickup = await _pickupRepo.GetByIdAsync(request.PickupRequestId);

            if (pickup == null)
                throw new Exception("Pickup request not found");

            if (!pickup.IsOtpVerified)
                throw new Exception("OTP not verified");

            pickup.Status = PickupStatus.VehiclePicked;

            var booking = await _bookingRepo.GetByIdAsync(pickup.BookingId);
            booking!.Status = BookingStatus.InLot;

            await _pickupRepo.UpdateAsync(pickup);
            await _bookingRepo.UpdateAsync(booking);

            return "Vehicle picked up successfully";
        }
    }
}

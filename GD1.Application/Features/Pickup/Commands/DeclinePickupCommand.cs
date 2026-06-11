using GD1.Application.Common;
using GD1.Domain.Entities;
using GD1.Domain.Entities.Enums;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.Pickup.Commands
{
    public class DeclinePickupCommand : IRequest<BaseResponse<bool>>
    {
        public long PickupRequestId { get; set; }
        public string? Reason { get; set; }
    }

    public class DeclinePickupCommandHandler : IRequestHandler<DeclinePickupCommand, BaseResponse<bool>>
    {
        private readonly IGenericRepository<PickupRequest> _pickupRepo;
        private readonly IGenericRepository<Booking> _bookingRepo;

        public DeclinePickupCommandHandler(
            IGenericRepository<PickupRequest> pickupRepo,
            IGenericRepository<Booking> bookingRepo)
        {
            _pickupRepo = pickupRepo;
            _bookingRepo = bookingRepo;
        }

        public async Task<BaseResponse<bool>> Handle(DeclinePickupCommand request, CancellationToken cancellationToken)
        {
            var pickup = await _pickupRepo.GetByIdAsync(request.PickupRequestId);
            if (pickup == null)
                return BaseResponse<bool>.Fail("Pickup request not found.");

            pickup.Status = PickupStatus.Declined;
            pickup.IsApprovedByLotOwner = false;

            await _pickupRepo.UpdateAsync(pickup);

            var booking = await _bookingRepo.GetByIdAsync(pickup.BookingId);
            if (booking != null)
            {
                // Update booking to reflect rejection
                booking.IsPickupRequested = false; 
                booking.RejectionReason = request.Reason;
                await _bookingRepo.UpdateAsync(booking);
            }

            return BaseResponse<bool>.Ok(true, "Pickup request declined successfully.");
        }
    }
}

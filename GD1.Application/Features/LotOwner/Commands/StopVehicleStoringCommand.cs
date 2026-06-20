using GD1.Application.Common;
using GD1.Domain.Entities;
using GD1.Domain.Entities.Enums;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.LotOwner.Commands
{
    public class StopVehicleStoringCommand : IRequest<BaseResponse<bool>>
    {
        public long LotOwnerId { get; set; }
        public long VehicleId { get; set; }
    }

    public class StopVehicleStoringCommandHandler : IRequestHandler<StopVehicleStoringCommand, BaseResponse<bool>>
    {
        private readonly IGenericRepository<Booking> _bookingRepo;
        private readonly IGenericRepository<VehicleJourneyEvent> _journeyRepo;
        private readonly IGenericRepository<StoredVehicle> _storedVehicleRepo;
        private readonly IGenericRepository<VehicleStorageSlot> _slotRepo;

        public StopVehicleStoringCommandHandler(
            IGenericRepository<Booking> bookingRepo,
            IGenericRepository<VehicleJourneyEvent> journeyRepo,
            IGenericRepository<StoredVehicle> storedVehicleRepo,
            IGenericRepository<VehicleStorageSlot> slotRepo)
        {
            _bookingRepo = bookingRepo;
            _journeyRepo = journeyRepo;
            _storedVehicleRepo = storedVehicleRepo;
            _slotRepo = slotRepo;
        }

        public async Task<BaseResponse<bool>> Handle(StopVehicleStoringCommand request, CancellationToken cancellationToken)
        {
            var bookings = await _bookingRepo.FindAsync(
                b => b.VehicleId == request.VehicleId
                     && b.Property.LotOwnerId == request.LotOwnerId
                     && b.Status != BookingStatus.Completed
                     && b.Status != BookingStatus.Cancelled,
                "Property"
            );

            var booking = bookings.FirstOrDefault();
            if (booking == null)
                return BaseResponse<bool>.Fail("Active booking not found for this vehicle under your lot.");

            booking.EndDate = DateTime.UtcNow;
            booking.Status = BookingStatus.Completed;
            await _bookingRepo.UpdateAsync(booking);

            // Set StoredVehicle to inactive so slot becomes available
            var storedVehicles = await _storedVehicleRepo.FindAsync(sv => sv.VehicleId == booking.VehicleId && sv.IsActive);
            foreach (var sv in storedVehicles)
            {
                sv.IsActive = false;
                await _storedVehicleRepo.UpdateAsync(sv);
            }

            // Mark the slot as unoccupied
            if (booking.SlotId.HasValue)
            {
                var slot = await _slotRepo.GetByIdAsync(booking.SlotId.Value);
                if (slot != null)
                {
                    slot.IsOccupied = false;
                    await _slotRepo.UpdateAsync(slot);
                }
            }

            // Add journey event
            var stopEvent = new VehicleJourneyEvent
            {
                BookingId = booking.Id,
                VehicleId = booking.VehicleId,
                EventType = "Vehicle Storage Stopped",
                Description = "Vehicle storage has been stopped by the lot owner.",
                TriggeredBy = request.LotOwnerId
            };
            await _journeyRepo.AddAsync(stopEvent);

            return BaseResponse<bool>.Ok(true);
        }
    }
}

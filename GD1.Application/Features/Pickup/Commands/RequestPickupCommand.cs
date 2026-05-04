using GD1.Domain.Entities;
using GD1.Domain.Entities.Enums;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GD1.Application.Features.Pickup.Commands
{
    public record RequestPickupCommand(
        long BookingId,
        DateTime RequestedPickupTime,
        string PickupAddress,
        double Latitude,
        double Longitude
    ) : IRequest<long>;

    public class RequestPickupCommandHandler
        : IRequestHandler<RequestPickupCommand, long>
    {
        private readonly IGenericRepository<PickupRequest> _pickupRepo;
        private readonly IGenericRepository<Booking> _bookingRepo;

        public RequestPickupCommandHandler(
            IGenericRepository<PickupRequest> pickupRepo,
            IGenericRepository<Booking> bookingRepo)
        {
            _pickupRepo = pickupRepo;
            _bookingRepo = bookingRepo;
        }

        public async Task<long> Handle(RequestPickupCommand request, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepo.GetByIdAsync(request.BookingId);

            if (booking == null)
                throw new Exception("Booking not found");

            booking.PickupAddress = request.PickupAddress;
            booking.PickupLatitude = request.Latitude;
            booking.PickupLongitude = request.Longitude;


            await _bookingRepo.UpdateAsync(booking);

            var pickup = new PickupRequest
            {
                BookingId = request.BookingId,
                RequestedPickupTime = request.RequestedPickupTime,
                Status = PickupStatus.Requested
            };

            await _pickupRepo.AddAsync(pickup);

            return pickup.Id;
        }
    }
}

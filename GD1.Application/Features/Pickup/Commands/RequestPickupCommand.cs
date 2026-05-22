using GD1.Domain.Entities;
using GD1.Domain.Entities.Enums;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using GD1.Application.Interfaces;

namespace GD1.Application.Features.Pickup.Commands
{
    public record RequestPickupCommand(
        long BookingId,
        string City,
        string PickupAddress,
        string? Pincode = null,
        DateTime? RequestedPickupTime = null  // null = ASAP
    ) : IRequest<GD1.Application.Common.BaseResponse<long>>;

    public class RequestPickupCommandValidator : AbstractValidator<RequestPickupCommand>
    {
        public RequestPickupCommandValidator()
        {
            RuleFor(x => x.BookingId).GreaterThan(0);
            RuleFor(x => x.City).NotEmpty().WithMessage("City is required.");
            RuleFor(x => x.PickupAddress).NotEmpty().WithMessage("Pickup address is required.");

            When(x => x.RequestedPickupTime.HasValue, () =>
            {
                RuleFor(x => x.RequestedPickupTime!.Value)
                    .GreaterThanOrEqualTo(DateTime.UtcNow.AddMinutes(-5))
                    .WithMessage("Requested pickup time cannot be in the past.");
            });
        }
    }

    public class RequestPickupCommandHandler
        : IRequestHandler<RequestPickupCommand, GD1.Application.Common.BaseResponse<long>>
    {
        private readonly IGenericRepository<PickupRequest> _pickupRepo;
        private readonly IGenericRepository<Booking> _bookingRepo;
        private readonly IGenericRepository<VehicleStorageProperty> _propertyRepo;
        private readonly IGeocodingService _geocodingService;

        public RequestPickupCommandHandler(
            IGenericRepository<PickupRequest> pickupRepo,
            IGenericRepository<Booking> bookingRepo,
            IGenericRepository<VehicleStorageProperty> propertyRepo,
            IGeocodingService geocodingService)
        {
            _pickupRepo = pickupRepo;
            _bookingRepo = bookingRepo;
            _propertyRepo = propertyRepo;
            _geocodingService = geocodingService;
        }

        public async Task<GD1.Application.Common.BaseResponse<long>> Handle(RequestPickupCommand request, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepo.GetByIdAsync(request.BookingId);
            if (booking == null)
                throw new Exception("Booking not found.");

            var property = await _propertyRepo.GetByIdAsync(booking.PropertyId);

            // Geocode the user-supplied address + city to get coordinates
            string fullAddress = string.Join(", ", new[]
            {
                request.PickupAddress,
                request.City,
                request.Pincode
            }.Where(s => !string.IsNullOrEmpty(s)));

            var coords = await _geocodingService.GetCoordinatesAsync(fullAddress);
            if (!coords.HasValue)
                throw new InvalidOperationException(
                    "Could not resolve coordinates for the provided address. Please check the address and city.");

            double resolvedLat = coords.Value.Lat;
            double resolvedLon = coords.Value.Lon;

            // Enforce 40km radius between user's pickup location and the storage property
            if (property?.Latitude.HasValue == true && property?.Longitude.HasValue == true)
            {
                double distKm = CalculateDistance(
                    resolvedLat, resolvedLon,
                    property.Latitude.Value, property.Longitude.Value);

                if (distKm > 40)
                    throw new InvalidOperationException(
                        $"Pickup is not available. Your location is {distKm:F1}km from the storage property. Pickup is only supported within a 40km radius.");
            }

            booking.PickupLatitude = resolvedLat;
            booking.PickupLongitude = resolvedLon;
            booking.PickupAddress = $"{request.PickupAddress}, {request.City}";
            booking.PickupPincode = request.Pincode;
            booking.IsPickupRequested = true;

            await _bookingRepo.UpdateAsync(booking);

            var pickup = new PickupRequest
            {
                BookingId = request.BookingId,
                RequestedPickupTime = request.RequestedPickupTime, // null = ASAP
                Status = PickupStatus.Approved, // Auto-approved since it passed 40km validation
                IsApprovedByLotOwner = true
            };

            await _pickupRepo.AddAsync(pickup);

            return GD1.Application.Common.BaseResponse<long>.Ok(pickup.Id, "Pickup requested successfully.");
        }

        private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371d;
            var dLat = (lat2 - lat1) * (Math.PI / 180d);
            var dLon = (lon2 - lon1) * (Math.PI / 180d);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1 * (Math.PI / 180d)) * Math.Cos(lat2 * (Math.PI / 180d)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            return R * 2d * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1d - a));
        }
    }
}

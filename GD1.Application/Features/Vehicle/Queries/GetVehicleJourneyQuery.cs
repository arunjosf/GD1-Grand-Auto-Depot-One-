using GD1.Application.Common;
using GD1.Domain.Entities;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.Vehicle.Queries
{
    public class GetVehicleJourneyQuery : IRequest<BaseResponse<IEnumerable<VehicleJourneyDto>>>
    {
        public long VehicleId { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }
        public long UserId { get; set; }
        public GD1.Domain.Entities.Enums.UserRole UserRole { get; set; }
    }

    public class VehicleJourneyDto
    {
        public long EventId { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<JourneyImageDto> Images { get; set; } = new();
    }

    public class JourneyImageDto
    {
        public string Label { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
    }

    public class GetVehicleJourneyQueryHandler : IRequestHandler<GetVehicleJourneyQuery, BaseResponse<IEnumerable<VehicleJourneyDto>>>
    {
        private readonly IGenericRepository<VehicleJourneyEvent> _journeyRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.Vehicle> _vehicleRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.Booking> _bookingRepo;

        public GetVehicleJourneyQueryHandler(
            IGenericRepository<VehicleJourneyEvent> journeyRepo,
            IGenericRepository<GD1.Domain.Entities.Vehicle> vehicleRepo,
            IGenericRepository<GD1.Domain.Entities.Booking> bookingRepo)
        {
            _journeyRepo = journeyRepo;
            _vehicleRepo = vehicleRepo;
            _bookingRepo = bookingRepo;
        }

        public async Task<BaseResponse<IEnumerable<VehicleJourneyDto>>> Handle(GetVehicleJourneyQuery request, CancellationToken cancellationToken)
        {
            var vehicle = await _vehicleRepo.GetByIdAsync(request.VehicleId);
            if (vehicle == null)
            {
                throw new Exception("Vehicle not found");
            }

            if (request.UserRole == GD1.Domain.Entities.Enums.UserRole.VehicleOwner && vehicle.OwnerId != request.UserId)
            {
                throw new UnauthorizedAccessException("You can only view the journey for your own vehicles.");
            }

            if (request.UserRole == GD1.Domain.Entities.Enums.UserRole.LotOwner)
            {
                var bookings = await _bookingRepo.FindAsync(b => b.VehicleId == request.VehicleId, "Property");
                if (!bookings.Any(b => b.Property?.LotOwnerId == request.UserId))
                {
                    throw new UnauthorizedAccessException("You can only view the journey for vehicles that are or were stored in your lot.");
                }
            }

            // Fetch all events for the vehicle and include images
            var events = await _journeyRepo.FindAsync(e => e.VehicleId == request.VehicleId, "Images");

            // Apply optional filters
            var filtered = events.AsEnumerable();
            if (request.Year.HasValue)
            {
                filtered = filtered.Where(e => e.CreatedAt.Year == request.Year.Value);
            }
            if (request.Month.HasValue)
            {
                filtered = filtered.Where(e => e.CreatedAt.Month == request.Month.Value);
            }

            var dtos = filtered.OrderBy(e => e.CreatedAt).Select(e => new VehicleJourneyDto
            {
                EventId = e.Id,
                EventType = e.EventType,
                Description = e.Description,
                CreatedAt = e.CreatedAt,
                Images = e.Images.Select(i => new JourneyImageDto
                {
                    Label = i.Label,
                    ImageUrl = i.ImageUrl
                }).ToList()
            });

            return BaseResponse<IEnumerable<VehicleJourneyDto>>.Ok(dtos);
        }
    }
}

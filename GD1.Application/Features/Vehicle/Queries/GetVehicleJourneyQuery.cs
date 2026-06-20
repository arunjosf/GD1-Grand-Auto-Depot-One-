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
        public long? BookingId { get; set; }
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
        public string? ActionUrl { get; set; }
        public string? ActionLabel { get; set; }
        
        // Manager details
        public string? ManagerName { get; set; }
        public string? ManagerAvatarUrl { get; set; }
        public string? ManagerRemarks { get; set; }
        public DateTime? VerifiedAt { get; set; }

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
        private readonly IGenericRepository<GD1.Domain.Entities.Agreement> _agreementRepo;
        private readonly IGenericRepository<PickupVerification> _pickupRepo;
        private readonly IGenericRepository<User> _userRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.LotManager> _lotManagerRepo;

        public GetVehicleJourneyQueryHandler(
            IGenericRepository<VehicleJourneyEvent> journeyRepo,
            IGenericRepository<GD1.Domain.Entities.Vehicle> vehicleRepo,
            IGenericRepository<GD1.Domain.Entities.Booking> bookingRepo,
            IGenericRepository<GD1.Domain.Entities.Agreement> agreementRepo,
            IGenericRepository<PickupVerification> pickupRepo,
            IGenericRepository<User> userRepo,
            IGenericRepository<GD1.Domain.Entities.LotManager> lotManagerRepo)
        {
            _journeyRepo = journeyRepo;
            _vehicleRepo = vehicleRepo;
            _bookingRepo = bookingRepo;
            _agreementRepo = agreementRepo;
            _pickupRepo = pickupRepo;
            _userRepo = userRepo;
            _lotManagerRepo = lotManagerRepo;
        }

        public async Task<BaseResponse<IEnumerable<VehicleJourneyDto>>> Handle(GetVehicleJourneyQuery request, CancellationToken cancellationToken)
        {
            var vehicles = await _vehicleRepo.FindAsync(v => v.Id == request.VehicleId, "Images");
            var vehicle = vehicles.FirstOrDefault();
            if (vehicle == null)
            {
                throw new Exception("Vehicle not found");
            }

            if (request.UserRole == GD1.Domain.Entities.Enums.UserRole.VehicleOwner && vehicle.OwnerId != request.UserId)
            {
                throw new UnauthorizedAccessException("You can only view the journey for your own vehicles.");
            }

            var bookings = (await _bookingRepo.FindAsync(b => b.VehicleId == request.VehicleId, "Property", "Slot")).ToList();
            if (request.UserRole == GD1.Domain.Entities.Enums.UserRole.LotOwner)
            {
                if (!bookings.Any(b => b.Property?.LotOwnerId == request.UserId))
                {
                    throw new UnauthorizedAccessException("You can only view the journey for vehicles that are or were stored in your lot.");
                }
            }

            // Determine target booking ID to prevent leakage of events from other bookings
            var targetBookingId = request.BookingId ?? bookings.OrderByDescending(b => b.CreatedAt).Select(b => b.Id).FirstOrDefault();

            // Fetch events for the vehicle and target booking and include images
            var events = await _journeyRepo.FindAsync(e => e.VehicleId == request.VehicleId && e.BookingId == targetBookingId, "Images");
            var bookingIds = bookings.Select(b => b.Id).ToList();
            var pickups = await _pickupRepo.FindAsync(p => bookingIds.Contains(p.BookingId));

            var dtos = new List<VehicleJourneyDto>();

            foreach (var e in events)
            {
                if (request.Year.HasValue && e.CreatedAt.Year != request.Year.Value) continue;
                if (request.Month.HasValue && e.CreatedAt.Month != request.Month.Value) continue;

                var dto = new VehicleJourneyDto
                {
                    EventId = e.Id,
                    EventType = e.EventType,
                    Description = e.Description,
                    CreatedAt = e.CreatedAt,
                    Images = e.Images.Select(i => new JourneyImageDto
                    {
                        Label = i.Label ?? "Image",
                        ImageUrl = i.ImageUrl
                    }).ToList()
                };

                // Enhance Pickup / Stored events with PickupVerification data if available
                if (e.BookingId.HasValue && (e.EventType == "VehiclePickedUp" || e.EventType == "VehicleStored" || e.EventType == "Pre-Ride Condition" || e.EventType == "Pickup Started" || e.EventType == "Pickup Requested" || e.EventType == "Arrived at Garage" || e.EventType == "Lot Arrival Condition" || e.EventType == "RideStarted"))
                {
                    var targetType = (e.EventType.Contains("Stored") || e.EventType.Contains("Arrival") || e.EventType.Contains("Lot")) ? GD1.Domain.Entities.Enums.ReportType.LotArrival : GD1.Domain.Entities.Enums.ReportType.Pickup;
                    var verification = pickups.FirstOrDefault(p => p.BookingId == e.BookingId.Value && p.Type == targetType);
                    if (verification != null)
                    {
                        var lotManager = await _lotManagerRepo.GetByIdAsync(verification.ManagerId);
                        var manager = lotManager != null ? await _userRepo.GetByIdAsync(lotManager.ManagerId) : null;
                        var managerName = manager?.FullName ?? "Unknown Manager";
                        
                        var defaultAvatar = "https://ui-avatars.com/api/?name=" + Uri.EscapeDataString(managerName) + "&background=0D8ABC&color=fff";
                        var avatarUrl = manager != null && !string.IsNullOrEmpty(manager.AvatarUrl) ? manager.AvatarUrl : defaultAvatar;
                        
                        dto.ManagerName = managerName;
                        dto.ManagerAvatarUrl = avatarUrl;
                        dto.VerifiedAt = verification.VerifiedAt;
                        dto.ManagerRemarks = verification.ManagerRemarks;

                        if (!string.IsNullOrEmpty(verification.SelfieUrl)) dto.Images.Add(new JourneyImageDto { Label = "Manager Selfie", ImageUrl = verification.SelfieUrl });
                        if (!string.IsNullOrEmpty(verification.FrontImageUrl)) dto.Images.Add(new JourneyImageDto { Label = "Front", ImageUrl = verification.FrontImageUrl });
                        if (!string.IsNullOrEmpty(verification.RearImageUrl)) dto.Images.Add(new JourneyImageDto { Label = "Rear", ImageUrl = verification.RearImageUrl });
                        if (!string.IsNullOrEmpty(verification.LeftSideImageUrl)) dto.Images.Add(new JourneyImageDto { Label = "Left Side", ImageUrl = verification.LeftSideImageUrl });
                        if (!string.IsNullOrEmpty(verification.RightSideImageUrl)) dto.Images.Add(new JourneyImageDto { Label = "Right Side", ImageUrl = verification.RightSideImageUrl });
                        if (!string.IsNullOrEmpty(verification.InteriorImageUrl)) dto.Images.Add(new JourneyImageDto { Label = "Interior", ImageUrl = verification.InteriorImageUrl });
                        if (!string.IsNullOrEmpty(verification.OdometerImageUrl)) dto.Images.Add(new JourneyImageDto { Label = "Odometer", ImageUrl = verification.OdometerImageUrl });
                    }
                }

                dtos.Add(dto);
            }

            var addSyntheticEvents = true;
            if (request.Year.HasValue && request.Year.Value != vehicle.CreatedAt.Year) addSyntheticEvents = false;
            if (request.Month.HasValue && request.Month.Value != vehicle.CreatedAt.Month) addSyntheticEvents = false;

            if (addSyntheticEvents || !request.Year.HasValue)
            {
                // Add "Vehicle Added" Event
                if (!request.Year.HasValue || (vehicle.CreatedAt.Year == request.Year.Value && (!request.Month.HasValue || vehicle.CreatedAt.Month == request.Month.Value)))
                {
                    var vehImages = vehicle.Images?.Select(i => new JourneyImageDto { Label = i.Label ?? "Vehicle Image", ImageUrl = i.ImageUrl }).ToList() ?? new List<JourneyImageDto>();
                    dtos.Add(new VehicleJourneyDto
                    {
                        EventId = -1,
                        EventType = "Vehicle Added",
                        Description = $"Vehicle Added\nBrand: {vehicle.Brand}\nModel: {vehicle.Model}\nRegistration: {vehicle.RegistrationNo}",
                        CreatedAt = vehicle.CreatedAt,
                        Images = vehImages
                    });
                }

                // Add "Booking Created" Events
                foreach (var b in bookings)
                {
                    if (request.Year.HasValue && b.CreatedAt.Year != request.Year.Value) continue;
                    if (request.Month.HasValue && b.CreatedAt.Month != request.Month.Value) continue;

                    var agreement = (await _agreementRepo.FindAsync(a => a.ReferenceId == b.Id && a.Type == GD1.Domain.Entities.Enums.AgreementType.LotBooking)).FirstOrDefault();
                    
                    // Note: EntityFramework Include for Property.Images might not be easy via FindAsync string includes if not configured deeply, 
                    // we'll rely on the frontend description for garage info if images fail to load deeply.
                    var slotName = b.Slot?.SlotNumber ?? "Unassigned Slot";
                    var garageName = b.Property?.Name ?? "Unknown Garage";
                    var addrLine = b.Property?.AddressLine;
                    var city = b.Property?.City;
                    var address = string.Join(", ", new[] { addrLine, city }.Where(s => !string.IsNullOrEmpty(s)));
                    if (string.IsNullOrEmpty(address)) address = "Unknown Address";

                    var bookingImages = new List<JourneyImageDto>();
                    if (!string.IsNullOrEmpty(b.Slot?.ImageUrl))
                    {
                        bookingImages.Add(new JourneyImageDto { Label = "Booked Slot", ImageUrl = b.Slot.ImageUrl });
                    }

                    dtos.Add(new VehicleJourneyDto
                    {
                        EventId = -(b.Id + 1000), // Ensure negative IDs don't conflict
                        EventType = "Booking Created",
                        Description = $"Booking created at {garageName}\nAddress: {address}\nSlot: {slotName}",
                        CreatedAt = b.CreatedAt,
                        ActionUrl = agreement != null ? $"https://localhost:7108/api/Agreement/{agreement.Id}/download" : null,
                        ActionLabel = agreement != null ? "Download Agreement" : null,
                        Images = bookingImages
                    });
                }
            }

            return BaseResponse<IEnumerable<VehicleJourneyDto>>.Ok(dtos.OrderBy(d => d.CreatedAt));
        }
    }
}

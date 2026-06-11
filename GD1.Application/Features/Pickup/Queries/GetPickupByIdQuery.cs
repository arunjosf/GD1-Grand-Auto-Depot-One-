using GD1.Application.Common;
using GD1.Domain.Entities;
using GD1.Domain.Entities.Enums;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace GD1.Application.Features.Pickup.Queries
{
    public class ConditionReportDetailsDto
    {
        public string? FrontImageUrl { get; set; }
        public string? RearImageUrl { get; set; }
        public string? LeftSideImageUrl { get; set; }
        public string? RightSideImageUrl { get; set; }
        public string? SelfieUrl { get; set; }
        public string? InteriorImageUrl { get; set; }
        public string? OdometerImageUrl { get; set; }
        public string? ManagerRemarks { get; set; }
    }

    public class PickupDetailsDto
    {
        public long PickupRequestId { get; set; }
        public long BookingId { get; set; }
        public DateTime BookingStartDate { get; set; }
        public DateTime? RequestedPickupTime { get; set; }
        public PickupStatus Status { get; set; }
        public bool IsApprovedByLotOwner { get; set; }

        public long CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;

        public string VehicleBrand { get; set; } = string.Empty;
        public string VehicleModel { get; set; } = string.Empty;
        public string RegistrationNo { get; set; } = string.Empty;
        public string? VehicleImage { get; set; }
        public string? VehicleRcUrl { get; set; }
        public string? OwnerIdProofUrl { get; set; }

        public string PickupAddress { get; set; } = string.Empty;
        public double? PickupLatitude { get; set; }
        public double? PickupLongitude { get; set; }

        public string LotAddress { get; set; } = string.Empty;
        public double? LotLatitude { get; set; }
        public double? LotLongitude { get; set; }
        public double? LastGpsLatitude { get; set; }
        public double? LastGpsLongitude { get; set; }

        // Manager details
        public string? ManagerName { get; set; }
        public string? ManagerEmail { get; set; }
        public string? ManagerPhone { get; set; }
        public string? ManagerSelfieUrl { get; set; }
        public string? ManagerIdProofUrl { get; set; }
        public DateTime? ManagerArrivalTime { get; set; }
        public long? PropertyId { get; set; }

        // Verification condition images/remarks
        public ConditionReportDetailsDto? PickupImages { get; set; }
        public ConditionReportDetailsDto? ArrivalImages { get; set; }
    }

    public class GetPickupByIdQuery : IRequest<BaseResponse<PickupDetailsDto>>
    {
        public long PickupRequestId { get; set; }
    }

    public class GetPickupByIdQueryHandler : IRequestHandler<GetPickupByIdQuery, BaseResponse<PickupDetailsDto>>
    {
        private readonly IGenericRepository<PickupRequest> _pickupRepo;
        private readonly IGenericRepository<Booking> _bookingRepo;
        private readonly IGenericRepository<VehicleStorageProperty> _propertyRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.Vehicle> _vehicleRepo;
        private readonly IGenericRepository<User> _userRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.LotManager> _lotManagerRepo;
        private readonly IGenericRepository<PickupVerification> _verificationRepo;
        private readonly IGenericRepository<JourneyLocation> _locationRepo;

        public GetPickupByIdQueryHandler(
            IGenericRepository<PickupRequest> pickupRepo,
            IGenericRepository<Booking> bookingRepo,
            IGenericRepository<VehicleStorageProperty> propertyRepo,
            IGenericRepository<GD1.Domain.Entities.Vehicle> vehicleRepo,
            IGenericRepository<User> userRepo,
            IGenericRepository<GD1.Domain.Entities.LotManager> lotManagerRepo,
            IGenericRepository<PickupVerification> verificationRepo,
            IGenericRepository<JourneyLocation> locationRepo)
        {
            _pickupRepo = pickupRepo;
            _bookingRepo = bookingRepo;
            _propertyRepo = propertyRepo;
            _vehicleRepo = vehicleRepo;
            _userRepo = userRepo;
            _lotManagerRepo = lotManagerRepo;
            _verificationRepo = verificationRepo;
            _locationRepo = locationRepo;
        }

        public async Task<BaseResponse<PickupDetailsDto>> Handle(GetPickupByIdQuery request, CancellationToken cancellationToken)
        {
            var pickup = await _pickupRepo.GetByIdAsync(request.PickupRequestId);
            if (pickup == null) return BaseResponse<PickupDetailsDto>.Fail("Pickup not found");

            var booking = await _bookingRepo.GetByIdAsync(pickup.BookingId);
            if (booking == null) return BaseResponse<PickupDetailsDto>.Fail("Booking not found");

            var property = await _propertyRepo.GetByIdAsync(booking.PropertyId);
            var vehicles = await _vehicleRepo.FindAsync(v => v.Id == booking.VehicleId, "Images");
            var vehicle = vehicles.FirstOrDefault();
            if (vehicle == null) return BaseResponse<PickupDetailsDto>.Fail("Vehicle not found");
            var owner = await _userRepo.GetByIdAsync(booking.OwnerId);

            var dto = new PickupDetailsDto
            {
                PickupRequestId = pickup.Id,
                BookingId = pickup.BookingId,
                BookingStartDate = booking.StartDate,
                RequestedPickupTime = pickup.RequestedPickupTime,
                Status = pickup.Status,
                IsApprovedByLotOwner = pickup.IsApprovedByLotOwner,
                CustomerId = owner.Id,
                CustomerName = owner.FullName,
                CustomerPhone = owner.PhoneNumber,
                VehicleBrand = vehicle.Brand,
                VehicleModel = vehicle.Model,
                RegistrationNo = vehicle.RegistrationNo,
                VehicleImage = vehicle.Images?.FirstOrDefault()?.ImageUrl,
                VehicleRcUrl = vehicle.VehicleRcUrl,
                OwnerIdProofUrl = vehicle.OwnerIdProofUrl,
                PickupAddress = booking.PickupAddress ?? "",
                PickupLatitude = booking.PickupLatitude,
                PickupLongitude = booking.PickupLongitude,
                LotAddress = property.AddressLine + ", " + property.City,
                LotLatitude = property.Latitude,
                LotLongitude = property.Longitude,
                PropertyId = booking.PropertyId
            };

            if (pickup.ManagerId.HasValue)
            {
                var lotManager = await _lotManagerRepo.GetByIdAsync(pickup.ManagerId.Value);
                if (lotManager != null)
                {
                    var managerUser = await _userRepo.GetByIdAsync(lotManager.ManagerId);
                    if (managerUser != null)
                    {
                        dto.ManagerName = managerUser.FullName;
                        dto.ManagerEmail = managerUser.Email;
                        dto.ManagerPhone = managerUser.PhoneNumber;
                        dto.ManagerSelfieUrl = lotManager.SelfieUrl;
                        dto.ManagerIdProofUrl = lotManager.IdProofUrl;
                    }
                }
                dto.ManagerArrivalTime = pickup.ManagerArrivalTime;
            }

            var verifications = await _verificationRepo.FindAsync(pv => pv.BookingId == booking.Id);
            var pickupVerif = verifications.FirstOrDefault(v => v.Type == ReportType.Pickup);
            var arrivalVerif = verifications.FirstOrDefault(v => v.Type == ReportType.LotArrival);

            if (pickupVerif != null)
            {
                dto.PickupImages = new ConditionReportDetailsDto
                {
                    FrontImageUrl = pickupVerif.FrontImageUrl,
                    RearImageUrl = pickupVerif.RearImageUrl,
                    LeftSideImageUrl = pickupVerif.LeftSideImageUrl,
                    RightSideImageUrl = pickupVerif.RightSideImageUrl,
                    SelfieUrl = pickupVerif.SelfieUrl,
                    InteriorImageUrl = pickupVerif.InteriorImageUrl,
                    OdometerImageUrl = pickupVerif.OdometerImageUrl,
                    ManagerRemarks = pickupVerif.ManagerRemarks
                };
            }

            if (arrivalVerif != null)
            {
                dto.ArrivalImages = new ConditionReportDetailsDto
                {
                    FrontImageUrl = arrivalVerif.FrontImageUrl,
                    RearImageUrl = arrivalVerif.RearImageUrl,
                    LeftSideImageUrl = arrivalVerif.LeftSideImageUrl,
                    RightSideImageUrl = arrivalVerif.RightSideImageUrl,
                    SelfieUrl = arrivalVerif.SelfieUrl,
                    InteriorImageUrl = arrivalVerif.InteriorImageUrl,
                    OdometerImageUrl = arrivalVerif.OdometerImageUrl,
                    ManagerRemarks = arrivalVerif.ManagerRemarks
                };
            }

            // Populate last known GPS location from JourneyLocation table
            var journeyLocations = await _locationRepo.FindAsync(l => l.BookingId == booking.Id);
            var lastLocation = journeyLocations.OrderByDescending(l => l.Timestamp).FirstOrDefault();
            if (lastLocation != null)
            {
                dto.LastGpsLatitude = lastLocation.Latitude;
                dto.LastGpsLongitude = lastLocation.Longitude;
            }

            return BaseResponse<PickupDetailsDto>.Ok(dto);
        }
    }
}

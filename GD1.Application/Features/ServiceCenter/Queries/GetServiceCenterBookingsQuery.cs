using GD1.Application.Common;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.ServiceCenter.Queries
{
    public class GetServiceCenterBookingsQuery : IRequest<BaseResponse<IEnumerable<ServiceBookingDto>>>
    {
        public long UserId { get; set; }
    }

    public class ServiceBookingDto
    {
        public long Id { get; set; }
        public long BookingId { get; set; }
        public long VehicleId { get; set; }
        public string VehicleBrand { get; set; } = string.Empty;
        public string VehicleModel { get; set; } = string.Empty;
        public string VehicleRegistrationNo { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string OwnerPhone { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime? RequestedDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ServiceCenterImage { get; set; }
        public decimal ServiceCost { get; set; }
        public bool IsCompleted { get; set; }
        
        public long? PropertyId { get; set; }
        public string? PropertyName { get; set; }
        public string? PropertyOwnerPhone { get; set; }
        public string? PropertyAddress { get; set; }
        public string? PropertyCity { get; set; }
        public double? PropertyLatitude { get; set; }
        public double? PropertyLongitude { get; set; }
        public double? ServiceCenterLatitude { get; set; }
        public double? ServiceCenterLongitude { get; set; }
    }

    public class GetServiceCenterBookingsQueryHandler : IRequestHandler<GetServiceCenterBookingsQuery, BaseResponse<IEnumerable<ServiceBookingDto>>>
    {
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceCenter> _scRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceRequest> _requestRepo;

        public GetServiceCenterBookingsQueryHandler(
            IGenericRepository<GD1.Domain.Entities.ServiceCenter> scRepo,
            IGenericRepository<GD1.Domain.Entities.ServiceRequest> requestRepo)
        {
            _scRepo = scRepo;
            _requestRepo = requestRepo;
        }

        public async Task<BaseResponse<IEnumerable<ServiceBookingDto>>> Handle(GetServiceCenterBookingsQuery request, CancellationToken ct)
        {
            // First find the service center owned by this user
            var centers = await _scRepo.FindAsync(sc => sc.AdminId == request.UserId);
            var center = centers.FirstOrDefault();

            if (center == null)
            {
                return BaseResponse<IEnumerable<ServiceBookingDto>>.Fail("Service Center not found for the current user.");
            }

            var serviceRequests = await _requestRepo.FindAsync(
                r => r.ServiceCenterId == center.Id, 
                "Booking.Vehicle.Owner", "Booking.Vehicle.Images", "Booking.Property.LotOwner"); // Need Booking, Vehicle, Owner, and Property details

            var dtos = serviceRequests.Select(r => new ServiceBookingDto
            {
                Id = r.Id,
                BookingId = r.BookingId,
                VehicleId = r.Booking?.VehicleId ?? 0,
                VehicleBrand = r.Booking?.Vehicle?.Brand ?? "Unknown",
                VehicleModel = r.Booking?.Vehicle?.Model ?? "Unknown",
                VehicleRegistrationNo = r.Booking?.Vehicle?.RegistrationNo ?? "Unknown",
                OwnerName = r.Booking?.Vehicle?.Owner?.FullName ?? "Unknown",
                OwnerPhone = r.Booking?.Vehicle?.Owner?.PhoneNumber ?? "Unknown",
                ServiceType = r.ServiceType,
                Notes = r.Notes,
                RequestedDate = r.ScheduledDate,
                Status = r.Status,
                ServiceCenterImage = r.Booking?.Vehicle?.Images?.FirstOrDefault()?.ImageUrl,
                ServiceCost = r.ServiceCost,
                IsCompleted = r.IsCompleted ?? false,
                PropertyId = r.Booking?.PropertyId,
                PropertyName = r.Booking?.Property?.Name,
                PropertyOwnerPhone = r.Booking?.Property?.LotOwner?.PhoneNumber,
                PropertyAddress = r.Booking?.Property?.AddressLine,
                PropertyCity = r.Booking?.Property?.City,
                PropertyLatitude = r.Booking?.Property?.Latitude,
                PropertyLongitude = r.Booking?.Property?.Longitude,
                ServiceCenterLatitude = center.Latitude,
                ServiceCenterLongitude = center.Longitude
            }).OrderByDescending(x => x.Id).ToList();

            return BaseResponse<IEnumerable<ServiceBookingDto>>.Ok(dtos);
        }
    }
}

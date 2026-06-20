using GD1.Application.Common;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.LotManager.Queries
{
    public class GetLotServiceBookingsQuery : IRequest<BaseResponse<IEnumerable<LotServiceBookingDto>>>
    {
        public long LotManagerId { get; set; }
        public long PropertyId { get; set; }       // Required: specific lot
        public long? ServiceRequestId { get; set; } // Optional: narrow to one booking
    }

    // Dedicated DTO with full service center details
    public class LotServiceBookingDto
    {
        public long Id { get; set; }
        public long BookingId { get; set; }
        public long VehicleId { get; set; }
        public string VehicleBrand { get; set; } = string.Empty;
        public string VehicleModel { get; set; } = string.Empty;
        public string VehicleRegistrationNo { get; set; } = string.Empty;
        public string? VehicleImage { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public string OwnerPhone { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal ServiceCost { get; set; }
        public string? MechanicEmail { get; set; }
        public string? CompletionNotes { get; set; }
        public string? BillUrl { get; set; }
        public bool IsCompleted { get; set; }

        // Storage lot details
        public long? PropertyId { get; set; }
        public string? PropertyName { get; set; }
        public string? PropertyCity { get; set; }

        // Assigned service center details
        public ServiceCenterSummaryDto? ServiceCenter { get; set; }
    }

    public class ServiceCenterSummaryDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public long OwnerId { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string AddressLine { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? SupportedBrands { get; set; }
        public decimal AverageRating { get; set; }
    }

    public class GetLotServiceBookingsQueryHandler : IRequestHandler<GetLotServiceBookingsQuery, BaseResponse<IEnumerable<LotServiceBookingDto>>>
    {
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceRequest> _requestRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.LotManager> _managerRepo;

        public GetLotServiceBookingsQueryHandler(
            IGenericRepository<GD1.Domain.Entities.ServiceRequest> requestRepo,
            IGenericRepository<GD1.Domain.Entities.LotManager> managerRepo)
        {
            _requestRepo = requestRepo;
            _managerRepo = managerRepo;
        }

        public async Task<BaseResponse<IEnumerable<LotServiceBookingDto>>> Handle(GetLotServiceBookingsQuery request, CancellationToken ct)
        {
            // Verify the manager has access to this specific property
            var managers = await _managerRepo.FindAsync(
                m => m.ManagerId == request.LotManagerId
                  && m.PropertyId == request.PropertyId
                  && m.IsActive
                  && m.ApprovalStatus == GD1.Domain.Entities.Enums.AgentApprovalStatus.Approved);

            if (!managers.Any())
                return BaseResponse<IEnumerable<LotServiceBookingDto>>.Fail("You do not have access to this property or it does not exist.");

            // Fetch service requests for bookings at this specific property
            var serviceRequests = await _requestRepo.FindAsync(
                r => r.Booking != null 
                    && !r.IsDeleted
                    && r.Booking.PropertyId == request.PropertyId
                    && (r.Status == "Approved" || r.Status == "Pending" || r.Status == "MechanicArrived"),
                "Booking.Vehicle.Owner",
                "Booking.Vehicle.Images",
                "Booking.Property",
                "ServiceCenter"
            );

            // Optionally filter down to one specific service request
            if (request.ServiceRequestId.HasValue)
                serviceRequests = serviceRequests.Where(r => r.Id == request.ServiceRequestId.Value);

            var dtos = serviceRequests.Select(r => new LotServiceBookingDto
            {
                Id = r.Id,
                BookingId = r.BookingId,
                VehicleId = r.Booking?.VehicleId ?? 0,
                VehicleBrand = r.Booking?.Vehicle?.Brand ?? "Unknown",
                VehicleModel = r.Booking?.Vehicle?.Model ?? "Unknown",
                VehicleRegistrationNo = r.Booking?.Vehicle?.RegistrationNo ?? "Unknown",
                VehicleImage = r.Booking?.Vehicle?.Images?.FirstOrDefault()?.ImageUrl,
                OwnerName = r.Booking?.Vehicle?.Owner?.FullName ?? "Unknown",
                OwnerPhone = r.Booking?.Vehicle?.Owner?.PhoneNumber ?? "Unknown",
                ServiceType = r.ServiceType,
                Notes = r.Notes,
                ScheduledDate = r.ScheduledDate,
                Status = r.Status,
                ServiceCost = r.ServiceCost,
                MechanicEmail = r.MechanicEmail,
                CompletionNotes = r.CompletionNotes,
                BillUrl = r.BillUrl,
                IsCompleted = r.IsCompleted ?? false,
                PropertyId = r.Booking?.PropertyId,
                PropertyName = r.Booking?.Property?.Name,
                PropertyCity = r.Booking?.Property?.City,
                ServiceCenter = r.ServiceCenter == null ? null : new ServiceCenterSummaryDto
                {
                    Id = r.ServiceCenter.Id,
                    OwnerId = r.ServiceCenter.AdminId,
                    Name = r.ServiceCenter.Name,
                    OwnerName = r.ServiceCenter.OwnerName,
                    PhoneNumber = r.ServiceCenter.PhoneNumber,
                    Email = r.ServiceCenter.Email,
                    AddressLine = r.ServiceCenter.AddressLine,
                    City = r.ServiceCenter.City,
                    District = r.ServiceCenter.District,
                    State = r.ServiceCenter.State,
                    Latitude = r.ServiceCenter.Latitude,
                    Longitude = r.ServiceCenter.Longitude,
                    SupportedBrands = r.ServiceCenter.SupportedBrands,
                    AverageRating = r.ServiceCenter.AverageRating
                }
            }).OrderBy(x => x.ScheduledDate ?? DateTime.MaxValue).ToList();

            return BaseResponse<IEnumerable<LotServiceBookingDto>>.Ok(dtos);
        }
    }
}

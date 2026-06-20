using GD1.Application.Common;
using GD1.Application.Features.LotManager.Queries; // Reuse LotServiceBookingDto
using GD1.Domain.Entities;
using GD1.Domain.Entities.Enums;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GD1.Application.Features.Bookings.Queries
{
    public class GetMyLotServicesQuery : IRequest<BaseResponse<IEnumerable<LotServiceBookingDto>>>
    {
        public long UserId { get; set; }
        public UserRole Role { get; set; }
    }

    public class GetMyLotServicesQueryHandler : IRequestHandler<GetMyLotServicesQuery, BaseResponse<IEnumerable<LotServiceBookingDto>>>
    {
        private readonly IGenericRepository<VehicleStorageProperty> _propertyRepo;
        private readonly IGenericRepository<global::GD1.Domain.Entities.LotManager> _managerRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.ServiceRequest> _requestRepo;

        public GetMyLotServicesQueryHandler(
            IGenericRepository<VehicleStorageProperty> propertyRepo,
            IGenericRepository<global::GD1.Domain.Entities.LotManager> managerRepo,
            IGenericRepository<GD1.Domain.Entities.ServiceRequest> requestRepo)
        {
            _propertyRepo = propertyRepo;
            _managerRepo = managerRepo;
            _requestRepo = requestRepo;
        }

        public async Task<BaseResponse<IEnumerable<LotServiceBookingDto>>> Handle(GetMyLotServicesQuery request, CancellationToken cancellationToken)
        {
            List<long> validPropertyIds = new List<long>();

            if (request.Role == UserRole.LotOwner)
            {
                var properties = await _propertyRepo.FindAsync(p => p.LotOwnerId == request.UserId && !p.IsDeleted);
                validPropertyIds = properties.Select(p => p.Id).ToList();
            }
            else if (request.Role == UserRole.Manager)
            {
                var managers = await _managerRepo.FindAsync(m => m.ManagerId == request.UserId && m.IsActive && !m.IsDeleted && m.ApprovalStatus == AgentApprovalStatus.Approved);
                validPropertyIds = managers.Select(m => m.PropertyId).ToList();
            }
            else
            {
                return BaseResponse<IEnumerable<LotServiceBookingDto>>.Fail("Invalid role for this query.");
            }

            var serviceRequests = await _requestRepo.FindAsync(
                r => r.Booking != null && validPropertyIds.Contains(r.Booking.PropertyId) && !r.IsDeleted,
                "Booking.Vehicle.Owner",
                "Booking.Vehicle.Images",
                "Booking.Property",
                "ServiceCenter"
            );

            var dtos = serviceRequests.OrderByDescending(r => r.CreatedAt).Select(r => new LotServiceBookingDto
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
            }).ToList();

            return BaseResponse<IEnumerable<LotServiceBookingDto>>.Ok(dtos);
        }
    }
}

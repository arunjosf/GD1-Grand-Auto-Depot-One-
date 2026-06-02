using GD1.Application.Common;
using GD1.Domain.Entities;
using GD1.Domain.Entities.Enums;
using GD1.Domain.Interfaces;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace GD1.Application.Features.GD1Admin.Queries
{
    public class AdminPropertyDrilldownDto
    {
        public long PropertyId { get; set; }
        public string PropertyName { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public int TotalSlots { get; set; }
        public int ActiveBookings { get; set; }
        public int VehiclesStored { get; set; }
        public decimal TotalRevenueGenerated { get; set; }
        public List<AdminPropertyBookingDto> RecentBookings { get; set; } = new();
    }

    public class AdminPropertyBookingDto
    {
        public long BookingId { get; set; }
        public string VehicleRegistration { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalCost { get; set; }
    }

    public class GetAdminPropertyDrilldownQuery : IRequest<BaseResponse<AdminPropertyDrilldownDto>>
    {
        public long PropertyId { get; set; }
    }

    public class GetAdminPropertyDrilldownQueryHandler : IRequestHandler<GetAdminPropertyDrilldownQuery, BaseResponse<AdminPropertyDrilldownDto>>
    {
        private readonly IGenericRepository<VehicleStorageProperty> _propertyRepo;
        private readonly IGenericRepository<User> _userRepo;
        private readonly IGenericRepository<VehicleStorageSlot> _slotRepo;
        private readonly IGenericRepository<Booking> _bookingRepo;
        private readonly IGenericRepository<StoredVehicle> _storedVehicleRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.Vehicle> _vehicleRepo;

        public GetAdminPropertyDrilldownQueryHandler(
            IGenericRepository<VehicleStorageProperty> propertyRepo,
            IGenericRepository<User> userRepo,
            IGenericRepository<VehicleStorageSlot> slotRepo,
            IGenericRepository<Booking> bookingRepo,
            IGenericRepository<StoredVehicle> storedVehicleRepo,
            IGenericRepository<GD1.Domain.Entities.Vehicle> vehicleRepo)
        {
            _propertyRepo = propertyRepo;
            _userRepo = userRepo;
            _slotRepo = slotRepo;
            _bookingRepo = bookingRepo;
            _storedVehicleRepo = storedVehicleRepo;
            _vehicleRepo = vehicleRepo;
        }

        public async Task<BaseResponse<AdminPropertyDrilldownDto>> Handle(GetAdminPropertyDrilldownQuery request, CancellationToken cancellationToken)
        {
            var property = await _propertyRepo.GetByIdAsync(request.PropertyId);
            if (property == null) return BaseResponse<AdminPropertyDrilldownDto>.Fail("Property not found.");

            var owner = await _userRepo.GetByIdAsync(property.LotOwnerId);
            var slots = await _slotRepo.FindAsync(s => s.PropertyId == property.Id);
            var bookings = await _bookingRepo.FindAsync(b => b.PropertyId == property.Id);
            var storedVehicles = await _storedVehicleRepo.FindAsync(sv => sv.PropertyId == property.Id && sv.IsActive);
            
            var allVehicles = await _vehicleRepo.GetAllAsync();

            var recentBookings = bookings
                .OrderByDescending(b => b.CreatedAt)
                .Take(10)
                .Select(b => new AdminPropertyBookingDto
                {
                    BookingId = b.Id,
                    VehicleRegistration = allVehicles.FirstOrDefault(v => v.Id == b.VehicleId)?.RegistrationNo ?? "Unknown",
                    StartDate = b.StartDate,
                    EndDate = b.EndDate,
                    Status = b.Status.ToString(),
                    TotalCost = b.TotalCost
                }).ToList();

            var dto = new AdminPropertyDrilldownDto
            {
                PropertyId = property.Id,
                PropertyName = property.Name,
                OwnerName = owner?.FullName ?? "Unknown",
                TotalSlots = slots.Count(),
                ActiveBookings = bookings.Count(b => b.Status == BookingStatus.InLot || b.Status == BookingStatus.AwaitingAgreement),
                VehiclesStored = storedVehicles.Count(),
                TotalRevenueGenerated = bookings.Where(b => b.Status == BookingStatus.Completed || b.Status == BookingStatus.InLot).Sum(b => b.TotalCost),
                RecentBookings = recentBookings
            };

            return BaseResponse<AdminPropertyDrilldownDto>.Ok(dto);
        }
    }
}

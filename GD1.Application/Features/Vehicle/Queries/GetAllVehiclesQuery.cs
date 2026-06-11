using GD1.Application.Common;
using GD1.Domain.Entities;
using GD1.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Application.Features.Vehicle.Queries
{
    public class GetAllVehiclesQuery : IRequest<BaseResponse<IEnumerable<VehicleWithDetailsDto>>>
    {
        public string? SearchTerm { get; set; }
        public long? PropertyOwnerId { get; set; }
    }

    public class VehicleWithDetailsDto
    {
        public long Id { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string RegistrationNo { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string PropertyName { get; set; } = string.Empty;
        public long? BookingId { get; set; }
        public string? Category { get; set; }
        public string? ProfileImageUrl { get; set; }
    }

    public class GetAllVehiclesQueryHandler : IRequestHandler<GetAllVehiclesQuery, BaseResponse<IEnumerable<VehicleWithDetailsDto>>>
    {
        private readonly IGenericRepository<GD1.Domain.Entities.Vehicle> _vehicleRepo;
        private readonly IGenericRepository<GD1.Domain.Entities.Booking> _bookingRepo;

        public GetAllVehiclesQueryHandler(
            IGenericRepository<GD1.Domain.Entities.Vehicle> vehicleRepo,
            IGenericRepository<GD1.Domain.Entities.Booking> bookingRepo)
        {
            _vehicleRepo = vehicleRepo;
            _bookingRepo = bookingRepo;
        }

        public async Task<BaseResponse<IEnumerable<VehicleWithDetailsDto>>> Handle(GetAllVehiclesQuery query, CancellationToken cancellationToken)
        {
            var vehicles = await _vehicleRepo.FindAsync(v => true, "Owner");
            var bookings = await _bookingRepo.FindAsync(b => true, "Vehicle", "Property");

            if (query.PropertyOwnerId.HasValue)
            {
                var myBookings = bookings.Where(b => b.Property?.LotOwnerId == query.PropertyOwnerId).ToList();
                var results = myBookings.Select(b => new VehicleWithDetailsDto
                {
                    Id = b.VehicleId,
                    Brand = b.Vehicle?.Brand ?? "Unknown",
                    Model = b.Vehicle?.Model ?? "Unknown",
                    RegistrationNo = b.Vehicle?.RegistrationNo ?? "Unknown",
                    OwnerName = b.Vehicle?.Owner?.FullName ?? "Unknown",
                    PropertyName = b.Property?.Name ?? "Unknown",
                    BookingId = b.Id,
                    Category = b.Vehicle?.Category,
                    ProfileImageUrl = b.Vehicle?.Images?.FirstOrDefault()?.ImageUrl
                }).ToList();

                if (!string.IsNullOrEmpty(query.SearchTerm))
                {
                    var term = query.SearchTerm.ToLower();
                    results = results.Where(v => 
                        v.OwnerName.ToLower().Contains(term) ||
                        v.Brand.ToLower().Contains(term) ||
                        v.Model.ToLower().Contains(term)
                    ).ToList();
                }

                return BaseResponse<IEnumerable<VehicleWithDetailsDto>>.Ok(results);
            }

            var allResults = vehicles.Select(v => {
                var booking = bookings.FirstOrDefault(b => b.VehicleId == v.Id);
                return new VehicleWithDetailsDto
                {
                    Id = v.Id,
                    Brand = v.Brand,
                    Model = v.Model,
                    RegistrationNo = v.RegistrationNo,
                    OwnerName = v.Owner?.FullName ?? "Unknown",
                    PropertyName = booking?.Property?.Name ?? "Unassigned"
                };
            }).ToList();

            if (!string.IsNullOrEmpty(query.SearchTerm))
            {
                var term = query.SearchTerm.ToLower();
                allResults = allResults.Where(v => 
                    v.OwnerName.ToLower().Contains(term) ||
                    v.Brand.ToLower().Contains(term) ||
                    v.Model.ToLower().Contains(term)
                ).ToList();
            }

            return BaseResponse<IEnumerable<VehicleWithDetailsDto>>.Ok(allResults);
        }
    }
}

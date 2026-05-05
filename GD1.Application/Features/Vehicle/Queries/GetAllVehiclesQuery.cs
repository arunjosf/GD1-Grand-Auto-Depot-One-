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
        public long? LotOwnerId { get; set; }
    }

    public class VehicleWithDetailsDto
    {
        public long Id { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string RegistrationNo { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string LotName { get; set; } = string.Empty;
        public string Plan { get; set; } = string.Empty;
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
            var vehicles = await _vehicleRepo.GetAllAsync();
            var bookings = await _bookingRepo.GetAllAsync();

            var result = bookings
                .Where(b => !query.LotOwnerId.HasValue || b.Lot.LotOwnerId == query.LotOwnerId)
                .ToList();

            if (!string.IsNullOrEmpty(query.SearchTerm))
            {
                var term = query.SearchTerm.ToLower();
                result = result.Where(b => 
                    (b.Vehicle?.Owner?.FullName?.ToLower().Contains(term) ?? false) ||
                    (b.Vehicle?.Brand?.ToLower().Contains(term) ?? false) ||
                    (b.Vehicle?.Model?.ToLower().Contains(term) ?? false)
                ).ToList();
            }

            var finalResult = result.Select(b => new VehicleWithDetailsDto
            {
                Id = b.VehicleId,
                Brand = b.Vehicle?.Brand ?? "Unknown",
                Model = b.Vehicle?.Model ?? "Unknown",
                RegistrationNo = b.Vehicle?.RegistrationNo ?? "Unknown",
                OwnerName = b.Vehicle?.Owner?.FullName ?? "Unknown",
                LotName = b.Lot?.Name ?? "Unknown",
                Plan = b.Plan
            }).ToList();

            return BaseResponse<IEnumerable<VehicleWithDetailsDto>>.Ok(finalResult);
        }
    }
}

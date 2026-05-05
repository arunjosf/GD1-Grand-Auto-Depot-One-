using GD1.Application.Common;
using GD1.Application.Features.Vehicle.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GD1.Application.Interfaces.Repositories;
using BCrypt;

namespace GD1.Application.Features.Vehicle.Queries
{
    public class GetMyVehiclesQuery
    {
        public long OwnerId { get; set; }
    }

    public class GetMyVehiclesQueryHandler
    {
        private readonly IVehicleReadRepository _repo;

        public GetMyVehiclesQueryHandler(IVehicleReadRepository repo)
            => _repo = repo;

        public async Task<BaseResponse<IEnumerable<VehicleDto>>> HandleAsync(
            GetMyVehiclesQuery query)
        {
            var vehicles = await _repo.GetByOwnerIdAsync(query.OwnerId);
            return BaseResponse<IEnumerable<VehicleDto>>.Ok(vehicles);
        }
    }

    public class GetVehicleDetailQuery
    {
        public long VehicleId { get; set; }
        public long OwnerId { get; set; }
    }

    public class GetVehicleDetailQueryHandler
    {
        private readonly IVehicleReadRepository _repo;

        public GetVehicleDetailQueryHandler(IVehicleReadRepository repo)
            => _repo = repo;

        public async Task<BaseResponse<VehicleDto>> HandleAsync(
            GetVehicleDetailQuery query)
        {
            var vehicle = await _repo.GetDetailAsync(
                query.VehicleId, query.OwnerId);

            if (vehicle is null)
                throw new KeyNotFoundException("Vehicle not found.");

            return BaseResponse<VehicleDto>.Ok(vehicle);
        }
    }
}

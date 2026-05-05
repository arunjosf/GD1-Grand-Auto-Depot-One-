using GD1.Application.Features.Vehicle.DTOs;
using GD1.Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace GD1.Infrastructure.Repositories
{
    public class VehicleReadRepository : IVehicleReadRepository
    {
        private readonly IDbConnection _db;

        public VehicleReadRepository(IDbConnection db) => _db = db;

        public async Task<IEnumerable<VehicleDto>> GetByOwnerIdAsync(long ownerId)
        {
            const string sql = @"
                SELECT Id, Brand, Model, Year, RegistrationNo,
                       Color, FuelType, VehicleType, HealthScore,
                       DocumentUrls, CreatedAt
                FROM   Vehicles
                WHERE  OwnerId = @OwnerId
                ORDER BY CreatedAt DESC";

            var vehicles = (await _db.QueryAsync<VehicleDto>(
                sql, new { OwnerId = ownerId })).ToList();

            foreach (var v in vehicles)
                v.Images = (await GetImagesAsync(v.Id)).ToList();

            return vehicles;
        }

        public async Task<VehicleDto?> GetDetailAsync(long vehicleId, long ownerId)
        {
            const string sql = @"
                SELECT Id, Brand, Model, Year, RegistrationNo,
                       Color, FuelType, VehicleType, HealthScore,
                       DocumentUrls, CreatedAt
                FROM   Vehicles
                WHERE  Id = @VehicleId AND OwnerId = @OwnerId";

            var vehicle = await _db.QuerySingleOrDefaultAsync<VehicleDto>(
                sql, new { VehicleId = vehicleId, OwnerId = ownerId });

            if (vehicle is null) return null;
            vehicle.Images = (await GetImagesAsync(vehicle.Id)).ToList();
            return vehicle;
        }

        private async Task<IEnumerable<VehicleImageDto>> GetImagesAsync(long vehicleId)
        {
            const string sql = @"
                SELECT Id, Label, ImageUrl, UploadedBy, Remark, EventId
                FROM   VehicleImages
                WHERE  VehicleId = @VehicleId";

            return await _db.QueryAsync<VehicleImageDto>(
                sql, new { VehicleId = vehicleId });
        }
    }
}

using GD1.Application.Features.Vehicle.DTOs;
using GD1.Application.Interfaces.Repositories;
using GD1.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GD1.Infrastructure.Repositories
{
    public class VehicleReadRepository : IVehicleReadRepository
    {
        private readonly AppDbContext _db;

        public VehicleReadRepository(AppDbContext db) => _db = db;

        public async Task<IEnumerable<VehicleDto>> GetByOwnerIdAsync(long ownerId)
        {
            var vehicles = await _db.Vehicles
                .Where(v => v.OwnerId == ownerId && !v.IsDeleted)
                .OrderByDescending(v => v.CreatedAt)
                .Select(v => new VehicleDto
                {
                    Id = v.Id,
                    Brand = v.Brand,
                    Model = v.Model,
                    Year = v.Year,
                    RegistrationNo = v.RegistrationNo,
                    Color = v.Color,
                    FuelType = v.FuelType,
                    VehicleType = v.VehicleType,
                    HealthScore = v.HealthScore,
                    DocumentUrls = v.DocumentUrls,
                    CreatedAt = v.CreatedAt,
                    LengthFeet = v.LengthFeet,
                    WidthFeet = v.WidthFeet,
                    HeightFeet = v.HeightFeet,
                    Images = v.Images.Select(img => new VehicleImageDto
                    {
                        Id = img.Id,
                        Label = img.Label,
                        ImageUrl = img.ImageUrl,
                        UploadedBy = img.UploadedBy,
                        Remark = img.Remark
                    }).ToList()
                })
                .ToListAsync();

            return vehicles;
        }

        public async Task<VehicleDto?> GetDetailAsync(long vehicleId, long ownerId)
        {
            var vehicle = await _db.Vehicles
                .Where(v => v.Id == vehicleId && v.OwnerId == ownerId && !v.IsDeleted)
                .Select(v => new VehicleDto
                {
                    Id = v.Id,
                    Brand = v.Brand,
                    Model = v.Model,
                    Year = v.Year,
                    RegistrationNo = v.RegistrationNo,
                    Color = v.Color,
                    FuelType = v.FuelType,
                    VehicleType = v.VehicleType,
                    HealthScore = v.HealthScore,
                    DocumentUrls = v.DocumentUrls,
                    CreatedAt = v.CreatedAt,
                    LengthFeet = v.LengthFeet,
                    WidthFeet = v.WidthFeet,
                    HeightFeet = v.HeightFeet,
                    Images = v.Images.Select(img => new VehicleImageDto
                    {
                        Id = img.Id,
                        Label = img.Label,
                        ImageUrl = img.ImageUrl,
                        UploadedBy = img.UploadedBy,
                        Remark = img.Remark
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            return vehicle;
        }
    }
}

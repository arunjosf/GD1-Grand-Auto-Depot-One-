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

        public async Task<IEnumerable<VehicleDto>> GetByOwnerIdAsync(long ownerId, long? vehicleId = null)
        {
            var query = _db.Vehicles
                .Where(v => v.OwnerId == ownerId && !v.IsDeleted);

            if (vehicleId.HasValue)
            {
                query = query.Where(v => v.Id == vehicleId.Value);
            }

            var vehicles = await query
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
                    IsHybrid = v.IsHybrid,
                    HealthScore = v.HealthScore,
                    OwnerIdProofUrl = v.OwnerIdProofUrl,
                    VehicleRcUrl = v.VehicleRcUrl,
                    CreatedAt = v.CreatedAt,
                    LengthFeet = v.LengthFeet,
                    WidthFeet = v.WidthFeet,
                    HeightFeet = v.HeightFeet,
                    IsStored = v.Bookings.Any(b => b.Status == GD1.Domain.Entities.Enums.BookingStatus.InLot),
                    LotName = v.Bookings.Where(b => b.Status == GD1.Domain.Entities.Enums.BookingStatus.InLot).Select(b => b.Property.Name).FirstOrDefault(),
                    Location = v.Bookings.Where(b => b.Status == GD1.Domain.Entities.Enums.BookingStatus.InLot).Select(b => b.Property.City).FirstOrDefault(),
                    StartDate = v.Bookings.Where(b => b.Status == GD1.Domain.Entities.Enums.BookingStatus.InLot).Select(b => (DateTime?)b.StartDate).FirstOrDefault(),
                    LastConditionUpdate = v.Bookings.Where(b => b.Status == GD1.Domain.Entities.Enums.BookingStatus.InLot)
                                            .SelectMany(b => b.JourneyEvents)
                                            .OrderByDescending(e => e.CreatedAt)
                                            .Select(e => (DateTime?)e.CreatedAt)
                                            .FirstOrDefault(),
                    PickupStatus = v.Bookings
                                    .SelectMany(b => b.PickupRequests)
                                    .OrderByDescending(pr => pr.CreatedAt)
                                    .Select(pr => pr.Status.ToString())
                                    .FirstOrDefault(),
                    JourneyEvents = v.Bookings.SelectMany(b => b.JourneyEvents)
                                     .OrderByDescending(e => e.CreatedAt)
                                     .Select(e => new VehicleJourneyEventDto
                                     {
                                         EventType = e.EventType,
                                         Description = e.Description,
                                         CreatedAt = e.CreatedAt,
                                         Images = e.Images.Select(img => new VehicleImageDto
                                         {
                                             Id = img.Id,
                                             Label = img.Label,
                                             ImageUrl = img.ImageUrl,
                                             UploadedBy = img.UploadedBy,
                                             EventId = img.EventId
                                         }).ToList()
                                     }).ToList(),
                    RecentOnDemandImages = v.Bookings.SelectMany(b => b.JourneyEvents)
                                            .Where(e => e.EventType == "OnDemandUpdate")
                                            .OrderByDescending(e => e.CreatedAt)
                                            .SelectMany(e => e.Images)
                                            .Select(img => new VehicleImageDto
                                            {
                                                Id = img.Id,
                                                Label = img.Label,
                                                ImageUrl = img.ImageUrl,
                                                UploadedBy = img.UploadedBy,
                                                EventId = img.EventId
                                            }).ToList(),
                    RecentWeeklyCheckImages = v.Bookings.SelectMany(b => b.JourneyEvents)
                                               .Where(e => e.EventType == "WeeklyUpdate" || e.EventType == "AdHocMaintenanceUpdate")
                                               .OrderByDescending(e => e.CreatedAt)
                                               .SelectMany(e => e.Images)
                                               .Select(img => new VehicleImageDto
                                               {
                                                   Id = img.Id,
                                                   Label = img.Label,
                                                   ImageUrl = img.ImageUrl,
                                                   UploadedBy = img.UploadedBy,
                                                   EventId = img.EventId
                                               }).ToList(),
                    PickupImages = v.Bookings.SelectMany(b => b.JourneyEvents)
                                    .Where(e => e.EventType == "VehiclePickedUp")
                                    .OrderByDescending(e => e.CreatedAt)
                                    .SelectMany(e => e.Images)
                                    .Select(img => new VehicleImageDto
                                    {
                                        Id = img.Id,
                                        Label = img.Label,
                                        ImageUrl = img.ImageUrl,
                                        UploadedBy = img.UploadedBy,
                                        EventId = img.EventId
                                    }).ToList(),
                    LotArrivalImages = v.Bookings.SelectMany(b => b.JourneyEvents)
                                        .Where(e => e.EventType == "VehicleStored")
                                        .OrderByDescending(e => e.CreatedAt)
                                        .SelectMany(e => e.Images)
                                        .Select(img => new VehicleImageDto
                                        {
                                            Id = img.Id,
                                            Label = img.Label,
                                            ImageUrl = img.ImageUrl,
                                            UploadedBy = img.UploadedBy,
                                            EventId = img.EventId
                                        }).ToList()
                })
                .ToListAsync();

            return vehicles;
        }
    }
}

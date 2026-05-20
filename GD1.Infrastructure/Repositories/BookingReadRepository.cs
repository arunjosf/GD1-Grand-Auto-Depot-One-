using Dapper;
using GD1.Application.Features.LotBooking.DTOs;
using GD1.Application.Interfaces.Repositories;
using GD1.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD1.Infrastructure.Repositories
{
    public class BookingReadRepository : IBookingReadRepository
    {
        private readonly IDbConnection _db;

        public BookingReadRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<IEnumerable<BookingDto>> GetByOwnerIdAsync(long ownerId)
        {
            const string sql = @"
                SELECT b.Id, b.VehicleId,
                       v.Brand AS VehicleBrand, v.Model AS VehicleModel,
                       v.RegistrationNo,
                       b.PropertyId, p.Name AS PropertyName,
                       p.AddressLine AS PropertyAddress,
                       s.SlotNumber,
                       b.StartDate, b.EndDate, b.Status,
                       b.PricePerDay, b.TotalCost,
                       b.CreatedAt
                FROM   Bookings b
                INNER JOIN Vehicles                 v ON b.VehicleId = v.Id
                INNER JOIN VehicleStorageProperties p ON b.PropertyId = p.Id
                LEFT  JOIN VehicleStorageSlots      s ON b.SlotId = s.Id
                WHERE  b.OwnerId = @OwnerId
                AND    b.Status NOT IN (0, 5, 6)
                ORDER BY b.CreatedAt DESC";

            return await _db.QueryAsync<BookingDto>(sql, new { OwnerId = ownerId });
        }

        public async Task<BookingDto?> GetDetailAsync(long bookingId, long ownerId)
        {
            const string sql = @"
                SELECT b.Id, b.VehicleId,
                       v.Brand AS VehicleBrand, v.Model AS VehicleModel,
                       v.RegistrationNo,
                       b.PropertyId, p.Name AS PropertyName,
                       p.AddressLine AS PropertyAddress,
                       s.SlotNumber,
                       b.StartDate, b.EndDate, b.Status,
                       b.PricePerDay, b.TotalCost,
                       b.CreatedAt
                FROM   Bookings b
                INNER JOIN Vehicles                 v ON b.VehicleId = v.Id
                INNER JOIN VehicleStorageProperties p ON b.PropertyId = p.Id
                LEFT  JOIN VehicleStorageSlots      s ON b.SlotId = s.Id
                WHERE  b.Id = @BookingId
                AND    b.OwnerId = @OwnerId
                AND    b.Status NOT IN (0, 5, 6)";

            return await _db.QuerySingleOrDefaultAsync<BookingDto>(sql, new { BookingId = bookingId, OwnerId = ownerId });
        }

        public async Task<IEnumerable<BookingDto>> GetByPropertyIdAsync(long propertyId)
        {
            const string sql = @"
                SELECT b.Id, b.VehicleId,
                       v.Brand AS VehicleBrand, v.Model AS VehicleModel,
                       v.RegistrationNo,
                       b.PropertyId, p.Name AS PropertyName,
                       p.AddressLine AS PropertyAddress,
                       s.SlotNumber,
                       b.StartDate, b.EndDate, b.Status,
                       b.PricePerDay, b.TotalCost,
                       b.CreatedAt
                FROM   Bookings b
                INNER JOIN Vehicles                 v ON b.VehicleId = v.Id
                INNER JOIN VehicleStorageProperties p ON b.PropertyId = p.Id
                LEFT  JOIN VehicleStorageSlots      s ON b.SlotId = s.Id
                WHERE  b.PropertyId = @PropertyId 
                AND    b.Status = @Status
                ORDER BY b.StartDate ASC";

            return await _db.QueryAsync<BookingDto>(sql, new
            {
                PropertyId = propertyId,
                Status = (int)BookingStatus.InLot
            });
        }

        public async Task<Dictionary<long, int>> GetOccupiedCountsAsync()
        {
            const string sql = @"
                SELECT PropertyId, COUNT(*) as OccupiedCount
                FROM   Bookings
                WHERE  Status NOT IN (3, 4) -- Completed, Cancelled
                GROUP BY PropertyId";

            var results = await _db.QueryAsync<(long PropertyId, int OccupiedCount)>(sql);
            return results.ToDictionary(x => x.PropertyId, x => x.OccupiedCount);
        }
    }
}

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
                       b.LotId, sl.Name AS LotName,
                       sl.AddressLine AS LotAddress,
                       ls.SlotNumber,
                       b.StartDate, b.EndDate, b.Plan, b.Status,
                       b.TotalCost,
                       b.CreatedAt
                FROM   Bookings b
                INNER JOIN Vehicles    v  ON b.VehicleId = v.Id
                INNER JOIN StorageLots sl ON b.LotId     = sl.Id
                LEFT  JOIN LotSlots    ls ON b.SlotId    = ls.Id
                WHERE  b.OwnerId = @OwnerId
                ORDER BY b.CreatedAt DESC";

            return await _db.QueryAsync<BookingDto>(
                sql, new { OwnerId = ownerId });
        }

        public async Task<BookingDto?> GetDetailAsync(long bookingId, long ownerId)
        {
            const string sql = @"
                SELECT b.Id, b.VehicleId,
                       v.Brand AS VehicleBrand, v.Model AS VehicleModel,
                       v.RegistrationNo,
                       b.LotId, sl.Name AS LotName,
                       sl.AddressLine AS LotAddress,
                       ls.SlotNumber,
                       b.StartDate, b.EndDate, b.Plan, b.Status,
                       b.TotalCost,
                       b.CreatedAt
                FROM   Bookings b
                INNER JOIN Vehicles    v  ON b.VehicleId = v.Id
                INNER JOIN StorageLots sl ON b.LotId     = sl.Id
                LEFT  JOIN LotSlots    ls ON b.SlotId    = ls.Id
                WHERE  b.Id = @BookingId 
                AND    b.OwnerId = @OwnerId";

            return await _db.QuerySingleOrDefaultAsync<BookingDto>(
                sql, new { BookingId = bookingId, OwnerId = ownerId });
        }

        public async Task<IEnumerable<BookingDto>> GetByLotIdAsync(long lotId)
        {
            const string sql = @"
                SELECT b.Id, b.VehicleId,
                       v.Brand AS VehicleBrand, v.Model AS VehicleModel,
                       v.RegistrationNo,
                       b.LotId, sl.Name AS LotName,
                       sl.AddressLine AS LotAddress,
                       ls.SlotNumber,
                       b.StartDate, b.EndDate, b.Plan, b.Status,
                       b.TotalCost,
                       b.CreatedAt
                FROM   Bookings b
                INNER JOIN Vehicles    v  ON b.VehicleId = v.Id
                INNER JOIN StorageLots sl ON b.LotId     = sl.Id
                LEFT  JOIN LotSlots    ls ON b.SlotId    = ls.Id
                WHERE  b.LotId = @LotId 
                AND    b.Status = @Status
                ORDER BY b.StartDate ASC";

            return await _db.QueryAsync<BookingDto>(
                sql,
                new
                {
                    LotId = lotId,
                    Status = (int)BookingStatus.InLot 
                });
        }
    }
}


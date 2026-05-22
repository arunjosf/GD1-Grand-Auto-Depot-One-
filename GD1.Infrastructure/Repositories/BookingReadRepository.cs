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
                       b.CreatedAt,
                       CASE pr.Status
                           WHEN 0 THEN 'Requested'
                           WHEN 1 THEN 'Assigned'
                           WHEN 2 THEN 'ManagerScheduled'
                           WHEN 3 THEN 'Approved'
                           WHEN 4 THEN 'OtpSent'
                           WHEN 5 THEN 'OwnerOtpSubmitted'
                           WHEN 6 THEN 'Verified'
                           WHEN 7 THEN 'VehiclePicked'
                           WHEN 8 THEN 'InTransit'
                           WHEN 9 THEN 'Stored'
                       END AS PickupStatus,
                       pr.ManagerArrivalTime,
                       mu.FullName AS ManagerName,
                       mu.PhoneNumber AS ManagerPhone,
                       lm.SelfieUrl AS ManagerSelfieUrl,
                       lm.IdProofUrl AS ManagerIdProofUrl,
                       pv_pickup.FrontImageUrl,
                       pv_pickup.RearImageUrl,
                       pv_pickup.LeftSideImageUrl,
                       pv_pickup.RightSideImageUrl,
                       pv_pickup.SelfieUrl,
                       pv_pickup.InteriorImageUrl,
                       pv_pickup.OdometerImageUrl,
                       pv_arrival.FrontImageUrl AS ArrivalFrontImageUrl,
                       pv_arrival.RearImageUrl AS ArrivalRearImageUrl,
                       pv_arrival.LeftSideImageUrl AS ArrivalLeftSideImageUrl,
                       pv_arrival.RightSideImageUrl AS ArrivalRightSideImageUrl,
                       pv_arrival.InteriorImageUrl AS ArrivalInteriorImageUrl,
                       pv_arrival.OdometerImageUrl AS ArrivalOdometerImageUrl
                FROM   Bookings b
                INNER JOIN Vehicles                 v ON b.VehicleId = v.Id
                INNER JOIN VehicleStorageProperties p ON b.PropertyId = p.Id
                LEFT  JOIN VehicleStorageSlots      s ON b.SlotId = s.Id
                LEFT  JOIN PickupRequests           pr ON b.Id = pr.BookingId
                LEFT  JOIN LotManagers              lm ON pr.ManagerId = lm.Id
                LEFT  JOIN Users                    mu ON lm.ManagerId = mu.Id
                LEFT JOIN PickupVerifications pv_pickup ON pv_pickup.BookingId = b.Id AND pv_pickup.Type = 0
                LEFT JOIN PickupVerifications pv_arrival ON pv_arrival.BookingId = b.Id AND pv_arrival.Type = 1
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
                       b.CreatedAt,
                       CASE pr.Status
                           WHEN 0 THEN 'Requested'
                           WHEN 1 THEN 'Assigned'
                           WHEN 2 THEN 'ManagerScheduled'
                           WHEN 3 THEN 'Approved'
                           WHEN 4 THEN 'OtpSent'
                           WHEN 5 THEN 'OwnerOtpSubmitted'
                           WHEN 6 THEN 'Verified'
                           WHEN 7 THEN 'VehiclePicked'
                           WHEN 8 THEN 'InTransit'
                           WHEN 9 THEN 'Stored'
                       END AS PickupStatus,
                       pr.ManagerArrivalTime,
                       mu.FullName AS ManagerName,
                       mu.PhoneNumber AS ManagerPhone,
                       lm.SelfieUrl AS ManagerSelfieUrl,
                       lm.IdProofUrl AS ManagerIdProofUrl,
                       pv_pickup.FrontImageUrl,
                       pv_pickup.RearImageUrl,
                       pv_pickup.LeftSideImageUrl,
                       pv_pickup.RightSideImageUrl,
                       pv_pickup.SelfieUrl,
                       pv_pickup.InteriorImageUrl,
                       pv_pickup.OdometerImageUrl,
                       pv_arrival.FrontImageUrl AS ArrivalFrontImageUrl,
                       pv_arrival.RearImageUrl AS ArrivalRearImageUrl,
                       pv_arrival.LeftSideImageUrl AS ArrivalLeftSideImageUrl,
                       pv_arrival.RightSideImageUrl AS ArrivalRightSideImageUrl,
                       pv_arrival.InteriorImageUrl AS ArrivalInteriorImageUrl,
                       pv_arrival.OdometerImageUrl AS ArrivalOdometerImageUrl
                FROM   Bookings b
                INNER JOIN Vehicles                 v ON b.VehicleId = v.Id
                INNER JOIN VehicleStorageProperties p ON b.PropertyId = p.Id
                LEFT  JOIN VehicleStorageSlots      s ON b.SlotId = s.Id
                LEFT  JOIN PickupRequests           pr ON b.Id = pr.BookingId
                LEFT  JOIN LotManagers              lm ON pr.ManagerId = lm.Id
                LEFT  JOIN Users                    mu ON lm.ManagerId = mu.Id
                LEFT JOIN PickupVerifications pv_pickup ON pv_pickup.BookingId = b.Id AND pv_pickup.Type = 0
                LEFT JOIN PickupVerifications pv_arrival ON pv_arrival.BookingId = b.Id AND pv_arrival.Type = 1
                WHERE  b.Id = @BookingId
                AND    b.OwnerId = @OwnerId
                AND    b.Status NOT IN (0, 5, 6)";

            return await _db.QuerySingleOrDefaultAsync<BookingDto>(sql, new { BookingId = bookingId, OwnerId = ownerId });
        }

        public async Task<IEnumerable<BookingDto>> GetByLotOwnerIdAsync(long lotOwnerId)
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
                       b.CreatedAt,
                       CASE pr.Status
                           WHEN 0 THEN 'Requested'
                           WHEN 1 THEN 'Assigned'
                           WHEN 2 THEN 'ManagerScheduled'
                           WHEN 3 THEN 'Approved'
                           WHEN 4 THEN 'OtpSent'
                           WHEN 5 THEN 'OwnerOtpSubmitted'
                           WHEN 6 THEN 'Verified'
                           WHEN 7 THEN 'VehiclePicked'
                           WHEN 8 THEN 'InTransit'
                           WHEN 9 THEN 'Stored'
                       END AS PickupStatus,
                       pr.ManagerArrivalTime,
                       mu.FullName AS ManagerName,
                       mu.PhoneNumber AS ManagerPhone,
                       lm.SelfieUrl AS ManagerSelfieUrl,
                       lm.IdProofUrl AS ManagerIdProofUrl,
                       pv_pickup.FrontImageUrl,
                       pv_pickup.RearImageUrl,
                       pv_pickup.LeftSideImageUrl,
                       pv_pickup.RightSideImageUrl,
                       pv_pickup.SelfieUrl,
                       pv_pickup.InteriorImageUrl,
                       pv_pickup.OdometerImageUrl,
                       pv_arrival.FrontImageUrl AS ArrivalFrontImageUrl,
                       pv_arrival.RearImageUrl AS ArrivalRearImageUrl,
                       pv_arrival.LeftSideImageUrl AS ArrivalLeftSideImageUrl,
                       pv_arrival.RightSideImageUrl AS ArrivalRightSideImageUrl,
                       pv_arrival.InteriorImageUrl AS ArrivalInteriorImageUrl,
                       pv_arrival.OdometerImageUrl AS ArrivalOdometerImageUrl
                FROM   Bookings b
                INNER JOIN Vehicles                 v ON b.VehicleId = v.Id
                INNER JOIN VehicleStorageProperties p ON b.PropertyId = p.Id
                LEFT  JOIN VehicleStorageSlots      s ON b.SlotId = s.Id
                LEFT  JOIN PickupRequests           pr ON b.Id = pr.BookingId
                LEFT  JOIN LotManagers              lm ON pr.ManagerId = lm.Id
                LEFT  JOIN Users                    mu ON lm.ManagerId = mu.Id
                LEFT JOIN PickupVerifications pv_pickup ON pv_pickup.BookingId = b.Id AND pv_pickup.Type = 0
                LEFT JOIN PickupVerifications pv_arrival ON pv_arrival.BookingId = b.Id AND pv_arrival.Type = 1
                WHERE  p.LotOwnerId = @LotOwnerId
                ORDER BY b.CreatedAt DESC";

            return await _db.QueryAsync<BookingDto>(sql, new { LotOwnerId = lotOwnerId });
        }

        public async Task<BookingDto?> GetLotOwnerBookingDetailAsync(long bookingId, long lotOwnerId)
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
                       b.CreatedAt,
                       CASE pr.Status
                           WHEN 0 THEN 'Requested'
                           WHEN 1 THEN 'Assigned'
                           WHEN 2 THEN 'ManagerScheduled'
                           WHEN 3 THEN 'Approved'
                           WHEN 4 THEN 'OtpSent'
                           WHEN 5 THEN 'OwnerOtpSubmitted'
                           WHEN 6 THEN 'Verified'
                           WHEN 7 THEN 'VehiclePicked'
                           WHEN 8 THEN 'InTransit'
                           WHEN 9 THEN 'Stored'
                       END AS PickupStatus,
                       pr.ManagerArrivalTime,
                       mu.FullName AS ManagerName,
                       mu.PhoneNumber AS ManagerPhone,
                       lm.SelfieUrl AS ManagerSelfieUrl,
                       lm.IdProofUrl AS ManagerIdProofUrl,
                       pv_pickup.FrontImageUrl,
                       pv_pickup.RearImageUrl,
                       pv_pickup.LeftSideImageUrl,
                       pv_pickup.RightSideImageUrl,
                       pv_pickup.SelfieUrl,
                       pv_pickup.InteriorImageUrl,
                       pv_pickup.OdometerImageUrl,
                       pv_arrival.FrontImageUrl AS ArrivalFrontImageUrl,
                       pv_arrival.RearImageUrl AS ArrivalRearImageUrl,
                       pv_arrival.LeftSideImageUrl AS ArrivalLeftSideImageUrl,
                       pv_arrival.RightSideImageUrl AS ArrivalRightSideImageUrl,
                       pv_arrival.InteriorImageUrl AS ArrivalInteriorImageUrl,
                       pv_arrival.OdometerImageUrl AS ArrivalOdometerImageUrl
                FROM   Bookings b
                INNER JOIN Vehicles                 v ON b.VehicleId = v.Id
                INNER JOIN VehicleStorageProperties p ON b.PropertyId = p.Id
                LEFT  JOIN VehicleStorageSlots      s ON b.SlotId = s.Id
                LEFT  JOIN PickupRequests           pr ON b.Id = pr.BookingId
                LEFT  JOIN LotManagers              lm ON pr.ManagerId = lm.Id
                LEFT  JOIN Users                    mu ON lm.ManagerId = mu.Id
                LEFT JOIN PickupVerifications pv_pickup ON pv_pickup.BookingId = b.Id AND pv_pickup.Type = 0
                LEFT JOIN PickupVerifications pv_arrival ON pv_arrival.BookingId = b.Id AND pv_arrival.Type = 1
                WHERE  b.Id = @BookingId
                AND    p.LotOwnerId = @LotOwnerId";

            return await _db.QuerySingleOrDefaultAsync<BookingDto>(sql, new { BookingId = bookingId, LotOwnerId = lotOwnerId });
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
                       b.CreatedAt,
                       CASE pr.Status
                           WHEN 0 THEN 'Requested'
                           WHEN 1 THEN 'Assigned'
                           WHEN 2 THEN 'ManagerScheduled'
                           WHEN 3 THEN 'Approved'
                           WHEN 4 THEN 'OtpSent'
                           WHEN 5 THEN 'OwnerOtpSubmitted'
                           WHEN 6 THEN 'Verified'
                           WHEN 7 THEN 'VehiclePicked'
                           WHEN 8 THEN 'InTransit'
                           WHEN 9 THEN 'Stored'
                       END AS PickupStatus,
                       pr.ManagerArrivalTime,
                       mu.FullName AS ManagerName,
                       mu.PhoneNumber AS ManagerPhone,
                       lm.SelfieUrl AS ManagerSelfieUrl,
                       lm.IdProofUrl AS ManagerIdProofUrl,
                       pv_pickup.FrontImageUrl,
                       pv_pickup.RearImageUrl,
                       pv_pickup.LeftSideImageUrl,
                       pv_pickup.RightSideImageUrl,
                       pv_pickup.SelfieUrl,
                       pv_pickup.InteriorImageUrl,
                       pv_pickup.OdometerImageUrl,
                       pv_arrival.FrontImageUrl AS ArrivalFrontImageUrl,
                       pv_arrival.RearImageUrl AS ArrivalRearImageUrl,
                       pv_arrival.LeftSideImageUrl AS ArrivalLeftSideImageUrl,
                       pv_arrival.RightSideImageUrl AS ArrivalRightSideImageUrl,
                       pv_arrival.InteriorImageUrl AS ArrivalInteriorImageUrl,
                       pv_arrival.OdometerImageUrl AS ArrivalOdometerImageUrl
                FROM   Bookings b
                INNER JOIN Vehicles                 v ON b.VehicleId = v.Id
                INNER JOIN VehicleStorageProperties p ON b.PropertyId = p.Id
                LEFT  JOIN VehicleStorageSlots      s ON b.SlotId = s.Id
                LEFT  JOIN PickupRequests           pr ON b.Id = pr.BookingId
                LEFT  JOIN LotManagers              lm ON pr.ManagerId = lm.Id
                LEFT  JOIN Users                    mu ON lm.ManagerId = mu.Id
                LEFT JOIN PickupVerifications pv_pickup ON pv_pickup.BookingId = b.Id AND pv_pickup.Type = 0
                LEFT JOIN PickupVerifications pv_arrival ON pv_arrival.BookingId = b.Id AND pv_arrival.Type = 1
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

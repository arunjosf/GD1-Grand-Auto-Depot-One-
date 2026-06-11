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
                       v.RegistrationNo, v.VehicleRcUrl, v.OwnerIdProofUrl,
                       (SELECT TOP 1 u.FullName FROM Users u WHERE u.Id = b.OwnerId) AS OwnerName,
                       b.PropertyId, p.Name AS PropertyName,
                       p.AddressLine AS PropertyAddress,
                       s.SlotNumber,
                       COALESCE(NULLIF(s.ImageUrl, ''), (SELECT TOP 1 pi.ImageUrl FROM PropertyImages pi WHERE pi.VehicleStoragePropertyId = p.Id ORDER BY pi.Id DESC)) AS PropertyImageUrl,
                       (SELECT TOP 1 vi.ImageUrl FROM VehicleImages vi WHERE vi.VehicleId = b.VehicleId ORDER BY vi.Id DESC) AS VehicleImageUrl,
                       b.StartDate, b.EndDate, b.Status,
                       b.PricePerDay, b.TotalCost,
                       b.PickupLatitude, b.PickupLongitude,
                       p.Latitude AS LotLatitude, p.Longitude AS LotLongitude,
                       (SELECT TOP 1 jl.Latitude FROM JourneyLocations jl WHERE jl.BookingId = b.Id ORDER BY jl.Timestamp DESC) AS LastGpsLatitude,
                       (SELECT TOP 1 jl.Longitude FROM JourneyLocations jl WHERE jl.BookingId = b.Id ORDER BY jl.Timestamp DESC) AS LastGpsLongitude,
                       b.CreatedAt, b.IsAgreementSigned, b.RejectionReason,
                       CASE pr.Status
                           WHEN 0 THEN 'Requested'
                           WHEN 1 THEN 'Assigned'
                           WHEN 2 THEN 'ManagerScheduled'
                           WHEN 3 THEN 'Approved'
                           WHEN 4 THEN 'Declined'
                           WHEN 5 THEN 'OtpSent'
                           WHEN 6 THEN 'OwnerOtpSubmitted'
                           WHEN 7 THEN 'Verified'
                           WHEN 8 THEN 'VehiclePicked'
                           WHEN 9 THEN 'InTransit'
                           WHEN 10 THEN 'Stored'
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
                       pv_pickup.OdometerImageUrl, pv_pickup.ManagerRemarks,
                       pv_arrival.FrontImageUrl AS ArrivalFrontImageUrl,
                       pv_arrival.RearImageUrl AS ArrivalRearImageUrl,
                       pv_arrival.LeftSideImageUrl AS ArrivalLeftSideImageUrl,
                       pv_arrival.RightSideImageUrl AS ArrivalRightSideImageUrl,
                       pv_arrival.InteriorImageUrl AS ArrivalInteriorImageUrl,
                       pv_arrival.OdometerImageUrl AS ArrivalOdometerImageUrl, pv_arrival.ManagerRemarks AS ArrivalManagerRemarks
                FROM   Bookings b
                INNER JOIN Vehicles                 v ON b.VehicleId = v.Id
                INNER JOIN VehicleStorageProperties p ON b.PropertyId = p.Id
                LEFT  JOIN VehicleStorageSlots      s ON b.SlotId = s.Id
                LEFT  JOIN PickupRequests           pr ON b.Id = pr.BookingId
                LEFT  JOIN LotManagers              lm ON pr.ManagerId = lm.Id
                LEFT  JOIN Users                    mu ON lm.ManagerId = mu.Id
                OUTER APPLY (SELECT TOP 1 * FROM PickupVerifications pv WHERE pv.BookingId = b.Id AND pv.Type = 0 ORDER BY pv.Id DESC) pv_pickup
                OUTER APPLY (SELECT TOP 1 * FROM PickupVerifications pv WHERE pv.BookingId = b.Id AND pv.Type = 1 ORDER BY pv.Id DESC) pv_arrival
                WHERE  b.OwnerId = @OwnerId
                AND    b.Status NOT IN (0, 5)
                ORDER BY b.CreatedAt DESC";

            return await _db.QueryAsync<BookingDto>(sql, new { OwnerId = ownerId });
        }

        public async Task<BookingDto?> GetDetailAsync(long bookingId, long ownerId)
        {
            const string sql = @"
                SELECT b.Id, b.VehicleId,
                       v.Brand AS VehicleBrand, v.Model AS VehicleModel,
                       v.RegistrationNo, v.VehicleRcUrl, v.OwnerIdProofUrl,
                       (SELECT TOP 1 u.FullName FROM Users u WHERE u.Id = b.OwnerId) AS OwnerName,
                       b.PropertyId, p.Name AS PropertyName,
                       p.AddressLine AS PropertyAddress,
                       s.SlotNumber,
                       COALESCE(NULLIF(s.ImageUrl, ''), (SELECT TOP 1 pi.ImageUrl FROM PropertyImages pi WHERE pi.VehicleStoragePropertyId = p.Id ORDER BY pi.Id DESC)) AS PropertyImageUrl,
                       (SELECT TOP 1 vi.ImageUrl FROM VehicleImages vi WHERE vi.VehicleId = b.VehicleId ORDER BY vi.Id DESC) AS VehicleImageUrl,
                       b.StartDate, b.EndDate, b.Status,
                       b.PricePerDay, b.TotalCost,
                       b.PickupLatitude, b.PickupLongitude,
                       p.Latitude AS LotLatitude, p.Longitude AS LotLongitude,
                       (SELECT TOP 1 jl.Latitude FROM JourneyLocations jl WHERE jl.BookingId = b.Id ORDER BY jl.Timestamp DESC) AS LastGpsLatitude,
                       (SELECT TOP 1 jl.Longitude FROM JourneyLocations jl WHERE jl.BookingId = b.Id ORDER BY jl.Timestamp DESC) AS LastGpsLongitude,
                       b.CreatedAt, b.IsAgreementSigned, b.RejectionReason,
                       CASE pr.Status
                           WHEN 0 THEN 'Requested'
                           WHEN 1 THEN 'Assigned'
                           WHEN 2 THEN 'ManagerScheduled'
                           WHEN 3 THEN 'Approved'
                           WHEN 4 THEN 'Declined'
                           WHEN 5 THEN 'OtpSent'
                           WHEN 6 THEN 'OwnerOtpSubmitted'
                           WHEN 7 THEN 'Verified'
                           WHEN 8 THEN 'VehiclePicked'
                           WHEN 9 THEN 'InTransit'
                           WHEN 10 THEN 'Stored'
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
                       pv_pickup.OdometerImageUrl, pv_pickup.ManagerRemarks,
                       pv_arrival.FrontImageUrl AS ArrivalFrontImageUrl,
                       pv_arrival.RearImageUrl AS ArrivalRearImageUrl,
                       pv_arrival.LeftSideImageUrl AS ArrivalLeftSideImageUrl,
                       pv_arrival.RightSideImageUrl AS ArrivalRightSideImageUrl,
                       pv_arrival.InteriorImageUrl AS ArrivalInteriorImageUrl,
                       pv_arrival.OdometerImageUrl AS ArrivalOdometerImageUrl, pv_arrival.ManagerRemarks AS ArrivalManagerRemarks
                FROM   Bookings b
                INNER JOIN Vehicles                 v ON b.VehicleId = v.Id
                INNER JOIN VehicleStorageProperties p ON b.PropertyId = p.Id
                LEFT  JOIN VehicleStorageSlots      s ON b.SlotId = s.Id
                LEFT  JOIN PickupRequests           pr ON b.Id = pr.BookingId
                LEFT  JOIN LotManagers              lm ON pr.ManagerId = lm.Id
                LEFT  JOIN Users                    mu ON lm.ManagerId = mu.Id
                OUTER APPLY (SELECT TOP 1 * FROM PickupVerifications pv WHERE pv.BookingId = b.Id AND pv.Type = 0 ORDER BY pv.Id DESC) pv_pickup
                OUTER APPLY (SELECT TOP 1 * FROM PickupVerifications pv WHERE pv.BookingId = b.Id AND pv.Type = 1 ORDER BY pv.Id DESC) pv_arrival
                WHERE  b.Id = @BookingId
                AND    b.OwnerId = @OwnerId
                AND    b.Status NOT IN (0, 5)";

            return await _db.QuerySingleOrDefaultAsync<BookingDto>(sql, new { BookingId = bookingId, OwnerId = ownerId });
        }

        public async Task<IEnumerable<BookingDto>> GetByLotOwnerIdAsync(long lotOwnerId)
        {
            const string sql = @"
                SELECT b.Id, b.VehicleId,
                       v.Brand AS VehicleBrand, v.Model AS VehicleModel,
                       v.RegistrationNo, v.VehicleRcUrl, v.OwnerIdProofUrl,
                       (SELECT TOP 1 u.FullName FROM Users u WHERE u.Id = b.OwnerId) AS OwnerName,
                       b.PropertyId, p.Name AS PropertyName,
                       p.AddressLine AS PropertyAddress,
                       s.SlotNumber,
                       COALESCE(NULLIF(s.ImageUrl, ''), (SELECT TOP 1 pi.ImageUrl FROM PropertyImages pi WHERE pi.VehicleStoragePropertyId = p.Id ORDER BY pi.Id DESC)) AS PropertyImageUrl,
                       (SELECT TOP 1 vi.ImageUrl FROM VehicleImages vi WHERE vi.VehicleId = b.VehicleId ORDER BY vi.Id DESC) AS VehicleImageUrl,
                       b.StartDate, b.EndDate, b.Status,
                       b.PricePerDay, b.TotalCost,
                       b.PickupLatitude, b.PickupLongitude,
                       p.Latitude AS LotLatitude, p.Longitude AS LotLongitude,
                       (SELECT TOP 1 jl.Latitude FROM JourneyLocations jl WHERE jl.BookingId = b.Id ORDER BY jl.Timestamp DESC) AS LastGpsLatitude,
                       (SELECT TOP 1 jl.Longitude FROM JourneyLocations jl WHERE jl.BookingId = b.Id ORDER BY jl.Timestamp DESC) AS LastGpsLongitude,
                       b.CreatedAt, b.IsAgreementSigned, b.RejectionReason,
                       CASE pr.Status
                           WHEN 0 THEN 'Requested'
                           WHEN 1 THEN 'Assigned'
                           WHEN 2 THEN 'ManagerScheduled'
                           WHEN 3 THEN 'Approved'
                           WHEN 4 THEN 'Declined'
                           WHEN 5 THEN 'OtpSent'
                           WHEN 6 THEN 'OwnerOtpSubmitted'
                           WHEN 7 THEN 'Verified'
                           WHEN 8 THEN 'VehiclePicked'
                           WHEN 9 THEN 'InTransit'
                           WHEN 10 THEN 'Stored'
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
                       pv_pickup.OdometerImageUrl, pv_pickup.ManagerRemarks,
                       pv_arrival.FrontImageUrl AS ArrivalFrontImageUrl,
                       pv_arrival.RearImageUrl AS ArrivalRearImageUrl,
                       pv_arrival.LeftSideImageUrl AS ArrivalLeftSideImageUrl,
                       pv_arrival.RightSideImageUrl AS ArrivalRightSideImageUrl,
                       pv_arrival.InteriorImageUrl AS ArrivalInteriorImageUrl,
                       pv_arrival.OdometerImageUrl AS ArrivalOdometerImageUrl, pv_arrival.ManagerRemarks AS ArrivalManagerRemarks
                FROM   Bookings b
                INNER JOIN Vehicles                 v ON b.VehicleId = v.Id
                INNER JOIN VehicleStorageProperties p ON b.PropertyId = p.Id
                LEFT  JOIN VehicleStorageSlots      s ON b.SlotId = s.Id
                LEFT  JOIN PickupRequests           pr ON b.Id = pr.BookingId
                LEFT  JOIN LotManagers              lm ON pr.ManagerId = lm.Id
                LEFT  JOIN Users                    mu ON lm.ManagerId = mu.Id
                OUTER APPLY (SELECT TOP 1 * FROM PickupVerifications pv WHERE pv.BookingId = b.Id AND pv.Type = 0 ORDER BY pv.Id DESC) pv_pickup
                OUTER APPLY (SELECT TOP 1 * FROM PickupVerifications pv WHERE pv.BookingId = b.Id AND pv.Type = 1 ORDER BY pv.Id DESC) pv_arrival
                WHERE  p.LotOwnerId = @LotOwnerId
                ORDER BY b.CreatedAt DESC";

            return await _db.QueryAsync<BookingDto>(sql, new { LotOwnerId = lotOwnerId });
        }

        public async Task<BookingDto?> GetLotOwnerBookingDetailAsync(long bookingId, long lotOwnerId)
        {
            const string sql = @"
                SELECT b.Id, b.VehicleId,
                       v.Brand AS VehicleBrand, v.Model AS VehicleModel,
                       v.RegistrationNo, v.VehicleRcUrl, v.OwnerIdProofUrl,
                       (SELECT TOP 1 u.FullName FROM Users u WHERE u.Id = b.OwnerId) AS OwnerName,
                       b.PropertyId, p.Name AS PropertyName,
                       p.AddressLine AS PropertyAddress,
                       s.SlotNumber,
                       COALESCE(NULLIF(s.ImageUrl, ''), (SELECT TOP 1 pi.ImageUrl FROM PropertyImages pi WHERE pi.VehicleStoragePropertyId = p.Id ORDER BY pi.Id DESC)) AS PropertyImageUrl,
                       (SELECT TOP 1 vi.ImageUrl FROM VehicleImages vi WHERE vi.VehicleId = b.VehicleId ORDER BY vi.Id DESC) AS VehicleImageUrl,
                       b.StartDate, b.EndDate, b.Status,
                       b.PricePerDay, b.TotalCost,
                       b.PickupLatitude, b.PickupLongitude,
                       p.Latitude AS LotLatitude, p.Longitude AS LotLongitude,
                       (SELECT TOP 1 jl.Latitude FROM JourneyLocations jl WHERE jl.BookingId = b.Id ORDER BY jl.Timestamp DESC) AS LastGpsLatitude,
                       (SELECT TOP 1 jl.Longitude FROM JourneyLocations jl WHERE jl.BookingId = b.Id ORDER BY jl.Timestamp DESC) AS LastGpsLongitude,
                       b.CreatedAt, b.IsAgreementSigned, b.RejectionReason,
                       CASE pr.Status
                           WHEN 0 THEN 'Requested'
                           WHEN 1 THEN 'Assigned'
                           WHEN 2 THEN 'ManagerScheduled'
                           WHEN 3 THEN 'Approved'
                           WHEN 4 THEN 'Declined'
                           WHEN 5 THEN 'OtpSent'
                           WHEN 6 THEN 'OwnerOtpSubmitted'
                           WHEN 7 THEN 'Verified'
                           WHEN 8 THEN 'VehiclePicked'
                           WHEN 9 THEN 'InTransit'
                           WHEN 10 THEN 'Stored'
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
                       pv_pickup.OdometerImageUrl, pv_pickup.ManagerRemarks,
                       pv_arrival.FrontImageUrl AS ArrivalFrontImageUrl,
                       pv_arrival.RearImageUrl AS ArrivalRearImageUrl,
                       pv_arrival.LeftSideImageUrl AS ArrivalLeftSideImageUrl,
                       pv_arrival.RightSideImageUrl AS ArrivalRightSideImageUrl,
                       pv_arrival.InteriorImageUrl AS ArrivalInteriorImageUrl,
                       pv_arrival.OdometerImageUrl AS ArrivalOdometerImageUrl, pv_arrival.ManagerRemarks AS ArrivalManagerRemarks
                FROM   Bookings b
                INNER JOIN Vehicles                 v ON b.VehicleId = v.Id
                INNER JOIN VehicleStorageProperties p ON b.PropertyId = p.Id
                LEFT  JOIN VehicleStorageSlots      s ON b.SlotId = s.Id
                LEFT  JOIN PickupRequests           pr ON b.Id = pr.BookingId
                LEFT  JOIN LotManagers              lm ON pr.ManagerId = lm.Id
                LEFT  JOIN Users                    mu ON lm.ManagerId = mu.Id
                OUTER APPLY (SELECT TOP 1 * FROM PickupVerifications pv WHERE pv.BookingId = b.Id AND pv.Type = 0 ORDER BY pv.Id DESC) pv_pickup
                OUTER APPLY (SELECT TOP 1 * FROM PickupVerifications pv WHERE pv.BookingId = b.Id AND pv.Type = 1 ORDER BY pv.Id DESC) pv_arrival
                WHERE  b.Id = @BookingId
                AND    p.LotOwnerId = @LotOwnerId";

            return await _db.QuerySingleOrDefaultAsync<BookingDto>(sql, new { BookingId = bookingId, LotOwnerId = lotOwnerId });
        }

        public async Task<BookingDto?> GetDetailAdminAsync(long bookingId)
        {
            const string sql = @"
                SELECT b.Id, b.VehicleId,
                       v.Brand AS VehicleBrand, v.Model AS VehicleModel,
                       v.RegistrationNo, v.VehicleRcUrl, v.OwnerIdProofUrl,
                       (SELECT TOP 1 u.FullName FROM Users u WHERE u.Id = b.OwnerId) AS OwnerName,
                       (SELECT TOP 1 u2.FullName FROM Users u2 WHERE u2.Id = p.OwnerId) AS LotOwnerName,
                       (SELECT TOP 1 u2.PhoneNumber FROM Users u2 WHERE u2.Id = p.OwnerId) AS LotOwnerPhone,
                       b.PropertyId, p.Name AS PropertyName,
                       p.AddressLine AS PropertyAddress,
                       s.SlotNumber,
                       COALESCE(NULLIF(s.ImageUrl, ''), (SELECT TOP 1 pi.ImageUrl FROM PropertyImages pi WHERE pi.VehicleStoragePropertyId = p.Id ORDER BY pi.Id DESC)) AS PropertyImageUrl,
                       (SELECT TOP 1 vi.ImageUrl FROM VehicleImages vi WHERE vi.VehicleId = b.VehicleId ORDER BY vi.Id DESC) AS VehicleImageUrl,
                       b.StartDate, b.EndDate, b.Status,
                       b.PricePerDay, b.TotalCost,
                       b.PickupLatitude, b.PickupLongitude,
                       p.Latitude AS LotLatitude, p.Longitude AS LotLongitude,
                       (SELECT TOP 1 jl.Latitude FROM JourneyLocations jl WHERE jl.BookingId = b.Id ORDER BY jl.Timestamp DESC) AS LastGpsLatitude,
                       (SELECT TOP 1 jl.Longitude FROM JourneyLocations jl WHERE jl.BookingId = b.Id ORDER BY jl.Timestamp DESC) AS LastGpsLongitude,
                       b.CreatedAt, b.IsAgreementSigned, b.RejectionReason,
                       CASE pr.Status
                           WHEN 0 THEN 'Requested'
                           WHEN 1 THEN 'Assigned'
                           WHEN 2 THEN 'ManagerScheduled'
                           WHEN 3 THEN 'Approved'
                           WHEN 4 THEN 'Declined'
                           WHEN 5 THEN 'OtpSent'
                           WHEN 6 THEN 'OwnerOtpSubmitted'
                           WHEN 7 THEN 'Verified'
                           WHEN 8 THEN 'VehiclePicked'
                           WHEN 9 THEN 'InTransit'
                           WHEN 10 THEN 'Stored'
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
                       pv_pickup.OdometerImageUrl, pv_pickup.ManagerRemarks,
                       pv_arrival.FrontImageUrl AS ArrivalFrontImageUrl,
                       pv_arrival.RearImageUrl AS ArrivalRearImageUrl,
                       pv_arrival.LeftSideImageUrl AS ArrivalLeftSideImageUrl,
                       pv_arrival.RightSideImageUrl AS ArrivalRightSideImageUrl,
                       pv_arrival.InteriorImageUrl AS ArrivalInteriorImageUrl,
                       pv_arrival.OdometerImageUrl AS ArrivalOdometerImageUrl, pv_arrival.ManagerRemarks AS ArrivalManagerRemarks
                FROM   Bookings b
                INNER JOIN Vehicles                 v ON b.VehicleId = v.Id
                INNER JOIN VehicleStorageProperties p ON b.PropertyId = p.Id
                LEFT  JOIN VehicleStorageSlots      s ON b.SlotId = s.Id
                LEFT  JOIN PickupRequests           pr ON b.Id = pr.BookingId
                LEFT  JOIN LotManagers              lm ON pr.ManagerId = lm.Id
                LEFT  JOIN Users                    mu ON lm.ManagerId = mu.Id
                OUTER APPLY (SELECT TOP 1 * FROM PickupVerifications pv WHERE pv.BookingId = b.Id AND pv.Type = 0 ORDER BY pv.Id DESC) pv_pickup
                OUTER APPLY (SELECT TOP 1 * FROM PickupVerifications pv WHERE pv.BookingId = b.Id AND pv.Type = 1 ORDER BY pv.Id DESC) pv_arrival
                WHERE  b.Id = @BookingId";

            return await _db.QuerySingleOrDefaultAsync<BookingDto>(sql, new { BookingId = bookingId });
        }

        public async Task<IEnumerable<BookingDto>> GetByPropertyIdAsync(long propertyId)
        {
            const string sql = @"
                SELECT b.Id, b.VehicleId,
                       v.Brand AS VehicleBrand, v.Model AS VehicleModel,
                       v.RegistrationNo, v.VehicleRcUrl, v.OwnerIdProofUrl,
                       (SELECT TOP 1 u.FullName FROM Users u WHERE u.Id = b.OwnerId) AS OwnerName,
                       (SELECT TOP 1 u2.FullName FROM Users u2 WHERE u2.Id = p.OwnerId) AS LotOwnerName,
                       (SELECT TOP 1 u2.PhoneNumber FROM Users u2 WHERE u2.Id = p.OwnerId) AS LotOwnerPhone,
                       b.PropertyId, p.Name AS PropertyName,
                       p.AddressLine AS PropertyAddress,
                       s.SlotNumber,
                       COALESCE(NULLIF(s.ImageUrl, ''), (SELECT TOP 1 pi.ImageUrl FROM PropertyImages pi WHERE pi.VehicleStoragePropertyId = p.Id ORDER BY pi.Id DESC)) AS PropertyImageUrl,
                       (SELECT TOP 1 vi.ImageUrl FROM VehicleImages vi WHERE vi.VehicleId = b.VehicleId ORDER BY vi.Id DESC) AS VehicleImageUrl,
                       b.StartDate, b.EndDate, b.Status,
                       b.PricePerDay, b.TotalCost,
                       b.PickupLatitude, b.PickupLongitude,
                       p.Latitude AS LotLatitude, p.Longitude AS LotLongitude,
                       (SELECT TOP 1 jl.Latitude FROM JourneyLocations jl WHERE jl.BookingId = b.Id ORDER BY jl.Timestamp DESC) AS LastGpsLatitude,
                       (SELECT TOP 1 jl.Longitude FROM JourneyLocations jl WHERE jl.BookingId = b.Id ORDER BY jl.Timestamp DESC) AS LastGpsLongitude,
                       b.CreatedAt, b.IsAgreementSigned, b.RejectionReason,
                       CASE pr.Status
                           WHEN 0 THEN 'Requested'
                           WHEN 1 THEN 'Assigned'
                           WHEN 2 THEN 'ManagerScheduled'
                           WHEN 3 THEN 'Approved'
                           WHEN 4 THEN 'Declined'
                           WHEN 5 THEN 'OtpSent'
                           WHEN 6 THEN 'OwnerOtpSubmitted'
                           WHEN 7 THEN 'Verified'
                           WHEN 8 THEN 'VehiclePicked'
                           WHEN 9 THEN 'InTransit'
                           WHEN 10 THEN 'Stored'
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
                       pv_pickup.OdometerImageUrl, pv_pickup.ManagerRemarks,
                       pv_arrival.FrontImageUrl AS ArrivalFrontImageUrl,
                       pv_arrival.RearImageUrl AS ArrivalRearImageUrl,
                       pv_arrival.LeftSideImageUrl AS ArrivalLeftSideImageUrl,
                       pv_arrival.RightSideImageUrl AS ArrivalRightSideImageUrl,
                       pv_arrival.InteriorImageUrl AS ArrivalInteriorImageUrl,
                       pv_arrival.OdometerImageUrl AS ArrivalOdometerImageUrl, pv_arrival.ManagerRemarks AS ArrivalManagerRemarks
                FROM   Bookings b
                INNER JOIN Vehicles                 v ON b.VehicleId = v.Id
                INNER JOIN VehicleStorageProperties p ON b.PropertyId = p.Id
                LEFT  JOIN VehicleStorageSlots      s ON b.SlotId = s.Id
                LEFT  JOIN PickupRequests           pr ON b.Id = pr.BookingId
                LEFT  JOIN LotManagers              lm ON pr.ManagerId = lm.Id
                LEFT  JOIN Users                    mu ON lm.ManagerId = mu.Id
                OUTER APPLY (SELECT TOP 1 * FROM PickupVerifications pv WHERE pv.BookingId = b.Id AND pv.Type = 0 ORDER BY pv.Id DESC) pv_pickup
                OUTER APPLY (SELECT TOP 1 * FROM PickupVerifications pv WHERE pv.BookingId = b.Id AND pv.Type = 1 ORDER BY pv.Id DESC) pv_arrival
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

        public async Task<IEnumerable<BookingDto>> GetAllAsync()
        {
            const string sql = @"
                SELECT b.Id, b.VehicleId,
                       v.Brand AS VehicleBrand, v.Model AS VehicleModel,
                       v.RegistrationNo, v.VehicleRcUrl, v.OwnerIdProofUrl,
                       (SELECT TOP 1 u.FullName FROM Users u WHERE u.Id = b.OwnerId) AS OwnerName,
                       (SELECT TOP 1 u2.FullName FROM Users u2 WHERE u2.Id = p.OwnerId) AS LotOwnerName,
                       (SELECT TOP 1 u2.PhoneNumber FROM Users u2 WHERE u2.Id = p.OwnerId) AS LotOwnerPhone,
                       b.PropertyId, p.Name AS PropertyName,
                       p.AddressLine AS PropertyAddress,
                       s.SlotNumber,
                       COALESCE(NULLIF(s.ImageUrl, ''), (SELECT TOP 1 pi.ImageUrl FROM PropertyImages pi WHERE pi.VehicleStoragePropertyId = p.Id ORDER BY pi.Id DESC)) AS PropertyImageUrl,
                       (SELECT TOP 1 vi.ImageUrl FROM VehicleImages vi WHERE vi.VehicleId = b.VehicleId ORDER BY vi.Id DESC) AS VehicleImageUrl,
                       b.StartDate, b.EndDate, b.Status,
                       b.PricePerDay, b.TotalCost,
                       b.PickupLatitude, b.PickupLongitude,
                       p.Latitude AS LotLatitude, p.Longitude AS LotLongitude,
                       (SELECT TOP 1 jl.Latitude FROM JourneyLocations jl WHERE jl.BookingId = b.Id ORDER BY jl.Timestamp DESC) AS LastGpsLatitude,
                       (SELECT TOP 1 jl.Longitude FROM JourneyLocations jl WHERE jl.BookingId = b.Id ORDER BY jl.Timestamp DESC) AS LastGpsLongitude,
                       b.CreatedAt, b.IsAgreementSigned, b.RejectionReason,
                       CASE pr.Status
                           WHEN 0 THEN 'Requested'
                           WHEN 1 THEN 'Assigned'
                           WHEN 2 THEN 'ManagerScheduled'
                           WHEN 3 THEN 'Approved'
                           WHEN 4 THEN 'Declined'
                           WHEN 5 THEN 'OtpSent'
                           WHEN 6 THEN 'OwnerOtpSubmitted'
                           WHEN 7 THEN 'Verified'
                           WHEN 8 THEN 'VehiclePicked'
                           WHEN 9 THEN 'InTransit'
                           WHEN 10 THEN 'Stored'
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
                       pv_pickup.OdometerImageUrl, pv_pickup.ManagerRemarks,
                       pv_arrival.FrontImageUrl AS ArrivalFrontImageUrl,
                       pv_arrival.RearImageUrl AS ArrivalRearImageUrl,
                       pv_arrival.LeftSideImageUrl AS ArrivalLeftSideImageUrl,
                       pv_arrival.RightSideImageUrl AS ArrivalRightSideImageUrl,
                       pv_arrival.InteriorImageUrl AS ArrivalInteriorImageUrl,
                       pv_arrival.OdometerImageUrl AS ArrivalOdometerImageUrl, pv_arrival.ManagerRemarks AS ArrivalManagerRemarks
                FROM   Bookings b
                INNER JOIN Vehicles                 v ON b.VehicleId = v.Id
                INNER JOIN VehicleStorageProperties p ON b.PropertyId = p.Id
                LEFT  JOIN VehicleStorageSlots      s ON b.SlotId = s.Id
                LEFT  JOIN PickupRequests           pr ON b.Id = pr.BookingId
                LEFT  JOIN LotManagers              lm ON pr.ManagerId = lm.Id
                LEFT  JOIN Users                    mu ON lm.ManagerId = mu.Id
                OUTER APPLY (SELECT TOP 1 * FROM PickupVerifications pv WHERE pv.BookingId = b.Id AND pv.Type = 0 ORDER BY pv.Id DESC) pv_pickup
                OUTER APPLY (SELECT TOP 1 * FROM PickupVerifications pv WHERE pv.BookingId = b.Id AND pv.Type = 1 ORDER BY pv.Id DESC) pv_arrival
                ORDER BY b.CreatedAt DESC";

            return await _db.QueryAsync<BookingDto>(sql);
        }
    }
}

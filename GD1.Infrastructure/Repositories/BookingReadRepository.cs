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
                       v.RegistrationNo, v.VehicleRcUrl, v.OwnerIdProofUrl, v.HasServiceRecommendation, v.ManagerServiceRemarks,
                       (SELECT TOP 1 sr.Id FROM ServiceRequests sr WHERE sr.BookingId = b.Id AND sr.Status NOT IN ('Completed', 'Cancelled', 'Service Completed', 'Payment') ORDER BY sr.Id DESC) AS ActiveServiceRequestId,
                       CAST(CASE WHEN EXISTS (SELECT 1 FROM ServiceRequests sr WHERE sr.BookingId = b.Id AND sr.Status NOT IN ('Completed', 'Cancelled', 'Service Completed', 'Payment')) THEN 1 ELSE 0 END AS BIT) AS HasActiveServiceRequest,
                       (SELECT TOP 1 mt.CompletedAt FROM MaintenanceTasks mt WHERE mt.VehicleId = v.Id AND mt.Type = 1 ORDER BY mt.CompletedAt DESC) AS LastServiceReportDate,
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
                       pv_arrival.OdometerImageUrl AS ArrivalOdometerImageUrl, pv_arrival.ManagerRemarks AS ArrivalManagerRemarks,
                       od.OnDemandFrontImageUrl,
                       od.OnDemandRearImageUrl,
                       od.OnDemandLeftSideImageUrl,
                       od.OnDemandRightSideImageUrl,
                       od.OnDemandInteriorImageUrl,
                       od.OnDemandOdometerImageUrl
                FROM   Bookings b
                INNER JOIN Vehicles                 v ON b.VehicleId = v.Id
                INNER JOIN VehicleStorageProperties p ON b.PropertyId = p.Id
                LEFT  JOIN VehicleStorageSlots      s ON b.SlotId = s.Id
                OUTER APPLY (SELECT TOP 1 * FROM PickupRequests pr2 WHERE pr2.BookingId = b.Id ORDER BY pr2.Id DESC) pr
                LEFT  JOIN LotManagers              lm ON pr.ManagerId = lm.Id
                LEFT  JOIN Users                    mu ON lm.ManagerId = mu.Id
                OUTER APPLY (SELECT TOP 1 * FROM PickupVerifications pv WHERE pv.BookingId = b.Id AND pv.Type = 0 ORDER BY pv.Id DESC) pv_pickup
                OUTER APPLY (SELECT TOP 1 * FROM PickupVerifications pv WHERE pv.BookingId = b.Id AND pv.Type = 1 ORDER BY pv.Id DESC) pv_arrival
                OUTER APPLY (
                    SELECT TOP 1 
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi WHERE vi.EventId = je.Id AND vi.Label = 'Front') AS OnDemandFrontImageUrl,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi WHERE vi.EventId = je.Id AND vi.Label = 'Rear') AS OnDemandRearImageUrl,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi WHERE vi.EventId = je.Id AND vi.Label = 'LeftSide') AS OnDemandLeftSideImageUrl,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi WHERE vi.EventId = je.Id AND vi.Label = 'RightSide') AS OnDemandRightSideImageUrl,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi WHERE vi.EventId = je.Id AND vi.Label = 'Interior') AS OnDemandInteriorImageUrl,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi WHERE vi.EventId = je.Id AND vi.Label = 'Odometer') AS OnDemandOdometerImageUrl
                    FROM VehicleJourneyEvents je
                    WHERE je.VehicleId = b.VehicleId AND je.EventType = 'OnDemandUpdate'
                    ORDER BY je.CreatedAt DESC
                ) od
                OUTER APPLY (
                    SELECT TOP 1 
                        je.Description AS WeeklyUpdateDescription,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi WHERE vi.EventId = je.Id AND vi.Label = 'Front') AS WeeklyUpdateFrontImageUrl,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi WHERE vi.EventId = je.Id AND vi.Label = 'Rear') AS WeeklyUpdateRearImageUrl,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi WHERE vi.EventId = je.Id AND vi.Label = 'LeftSide') AS WeeklyUpdateLeftSideImageUrl,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi WHERE vi.EventId = je.Id AND vi.Label = 'RightSide') AS WeeklyUpdateRightSideImageUrl,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi WHERE vi.EventId = je.Id AND vi.Label = 'Interior') AS WeeklyUpdateInteriorImageUrl,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi WHERE vi.EventId = je.Id AND vi.Label = 'Odometer') AS WeeklyUpdateOdometerImageUrl
                    FROM VehicleJourneyEvents je
                    WHERE je.VehicleId = b.VehicleId AND je.EventType IN ('WeeklyUpdate', 'AdHocMaintenanceUpdate')
                    ORDER BY je.CreatedAt DESC
                ) wu
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
                       v.RegistrationNo, v.VehicleRcUrl, v.OwnerIdProofUrl, v.HasServiceRecommendation, v.ManagerServiceRemarks,
                       (SELECT TOP 1 sr.Id FROM ServiceRequests sr WHERE sr.BookingId = b.Id AND sr.Status NOT IN ('Completed', 'Cancelled', 'Service Completed', 'Payment') ORDER BY sr.Id DESC) AS ActiveServiceRequestId,
                       CAST(CASE WHEN EXISTS (SELECT 1 FROM ServiceRequests sr WHERE sr.BookingId = b.Id AND sr.Status NOT IN ('Completed', 'Cancelled', 'Service Completed', 'Payment')) THEN 1 ELSE 0 END AS BIT) AS HasActiveServiceRequest,
                       (SELECT TOP 1 mt.CompletedAt FROM MaintenanceTasks mt WHERE mt.VehicleId = v.Id AND mt.Type = 1 ORDER BY mt.CompletedAt DESC) AS LastServiceReportDate,
                       CAST(CASE WHEN EXISTS (SELECT 1 FROM MaintenanceTasks mt WHERE mt.VehicleId = v.Id AND mt.Type = 0 AND mt.Status = 0) THEN 1 ELSE 0 END AS BIT) AS HasPendingOnDemandRequest,
                       (SELECT TOP 1 mt.RequestedAt FROM MaintenanceTasks mt WHERE mt.VehicleId = v.Id AND mt.Type = 0 AND mt.Status = 0 ORDER BY mt.RequestedAt DESC) AS PendingOnDemandRequestDate,
                       (SELECT TOP 1 u.FullName FROM Users u WHERE u.Id = b.OwnerId) AS OwnerName,
                       (SELECT TOP 1 u2.FullName FROM Users u2 WHERE u2.Id = p.LotOwnerId) AS LotOwnerName,
                       (SELECT TOP 1 u2.PhoneNumber FROM Users u2 WHERE u2.Id = p.LotOwnerId) AS LotOwnerPhone,
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
                       pv_arrival.OdometerImageUrl AS ArrivalOdometerImageUrl, pv_arrival.ManagerRemarks AS ArrivalManagerRemarks,
                       od.OnDemandFrontImageUrl, od.OnDemandRearImageUrl, od.OnDemandLeftSideImageUrl, od.OnDemandRightSideImageUrl, od.OnDemandInteriorImageUrl, od.OnDemandOdometerImageUrl,
                       wu.WeeklyUpdateDescription, wu.WeeklyUpdateFrontImageUrl, wu.WeeklyUpdateRearImageUrl, wu.WeeklyUpdateLeftSideImageUrl, wu.WeeklyUpdateRightSideImageUrl, wu.WeeklyUpdateInteriorImageUrl, wu.WeeklyUpdateOdometerImageUrl
                FROM   Bookings b
                INNER JOIN Vehicles                 v ON b.VehicleId = v.Id
                INNER JOIN VehicleStorageProperties p ON b.PropertyId = p.Id
                LEFT  JOIN VehicleStorageSlots      s ON b.SlotId = s.Id
                OUTER APPLY (SELECT TOP 1 * FROM PickupRequests pr2 WHERE pr2.BookingId = b.Id ORDER BY pr2.Id DESC) pr
                LEFT  JOIN LotManagers              lm ON pr.ManagerId = lm.Id
                LEFT  JOIN Users                    mu ON lm.ManagerId = mu.Id
                OUTER APPLY (SELECT TOP 1 * FROM PickupVerifications pv WHERE pv.BookingId = b.Id AND pv.Type = 0 ORDER BY pv.Id DESC) pv_pickup
                OUTER APPLY (SELECT TOP 1 * FROM PickupVerifications pv WHERE pv.BookingId = b.Id AND pv.Type = 1 ORDER BY pv.Id DESC) pv_arrival
                OUTER APPLY (
                    SELECT TOP 1 
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi WHERE vi.EventId = je.Id AND vi.Label = 'Front') AS OnDemandFrontImageUrl,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi WHERE vi.EventId = je.Id AND vi.Label = 'Rear') AS OnDemandRearImageUrl,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi WHERE vi.EventId = je.Id AND vi.Label = 'LeftSide') AS OnDemandLeftSideImageUrl,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi WHERE vi.EventId = je.Id AND vi.Label = 'RightSide') AS OnDemandRightSideImageUrl,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi WHERE vi.EventId = je.Id AND vi.Label = 'Interior') AS OnDemandInteriorImageUrl,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi WHERE vi.EventId = je.Id AND vi.Label = 'Odometer') AS OnDemandOdometerImageUrl
                    FROM VehicleJourneyEvents je
                    WHERE je.VehicleId = b.VehicleId AND je.EventType = 'OnDemandUpdate'
                    ORDER BY je.CreatedAt DESC
                ) od
                OUTER APPLY (
                    SELECT TOP 1 
                        je.Description AS WeeklyUpdateDescription,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi WHERE vi.EventId = je.Id AND vi.Label = 'Front') AS WeeklyUpdateFrontImageUrl,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi WHERE vi.EventId = je.Id AND vi.Label = 'Rear') AS WeeklyUpdateRearImageUrl,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi WHERE vi.EventId = je.Id AND vi.Label = 'LeftSide') AS WeeklyUpdateLeftSideImageUrl,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi WHERE vi.EventId = je.Id AND vi.Label = 'RightSide') AS WeeklyUpdateRightSideImageUrl,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi WHERE vi.EventId = je.Id AND vi.Label = 'Interior') AS WeeklyUpdateInteriorImageUrl,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi WHERE vi.EventId = je.Id AND vi.Label = 'Odometer') AS WeeklyUpdateOdometerImageUrl
                    FROM VehicleJourneyEvents je
                    WHERE je.VehicleId = b.VehicleId AND je.EventType IN ('WeeklyUpdate', 'AdHocMaintenanceUpdate')
                    ORDER BY je.CreatedAt DESC
                ) wu
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
                       v.RegistrationNo, v.VehicleRcUrl, v.OwnerIdProofUrl, v.HasServiceRecommendation, v.ManagerServiceRemarks,
                       (SELECT TOP 1 sr.Id FROM ServiceRequests sr WHERE sr.BookingId = b.Id AND sr.Status NOT IN ('Completed', 'Cancelled', 'Service Completed', 'Payment') ORDER BY sr.Id DESC) AS ActiveServiceRequestId,
                       CAST(CASE WHEN EXISTS (SELECT 1 FROM ServiceRequests sr WHERE sr.BookingId = b.Id AND sr.Status NOT IN ('Completed', 'Cancelled', 'Service Completed', 'Payment')) THEN 1 ELSE 0 END AS BIT) AS HasActiveServiceRequest,
                       (SELECT TOP 1 mt.CompletedAt FROM MaintenanceTasks mt WHERE mt.VehicleId = v.Id AND mt.Type = 1 ORDER BY mt.CompletedAt DESC) AS LastServiceReportDate,
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
                       pv_arrival.OdometerImageUrl AS ArrivalOdometerImageUrl, pv_arrival.ManagerRemarks AS ArrivalManagerRemarks,
                       od.OnDemandFrontImageUrl, od.OnDemandRearImageUrl, od.OnDemandLeftSideImageUrl, od.OnDemandRightSideImageUrl, od.OnDemandInteriorImageUrl, od.OnDemandOdometerImageUrl,
                       wu.WeeklyUpdateDescription, wu.WeeklyUpdateFrontImageUrl, wu.WeeklyUpdateRearImageUrl, wu.WeeklyUpdateLeftSideImageUrl, wu.WeeklyUpdateRightSideImageUrl, wu.WeeklyUpdateInteriorImageUrl, wu.WeeklyUpdateOdometerImageUrl
                FROM   Bookings b
                INNER JOIN Vehicles                 v ON b.VehicleId = v.Id
                INNER JOIN VehicleStorageProperties p ON b.PropertyId = p.Id
                LEFT  JOIN VehicleStorageSlots      s ON b.SlotId = s.Id
                OUTER APPLY (SELECT TOP 1 * FROM PickupRequests pr2 WHERE pr2.BookingId = b.Id ORDER BY pr2.Id DESC) pr
                LEFT  JOIN LotManagers              lm ON pr.ManagerId = lm.Id
                LEFT  JOIN Users                    mu ON lm.ManagerId = mu.Id
                OUTER APPLY (SELECT TOP 1 * FROM PickupVerifications pv WHERE pv.BookingId = b.Id AND pv.Type = 0 ORDER BY pv.Id DESC) pv_pickup
                OUTER APPLY (SELECT TOP 1 * FROM PickupVerifications pv WHERE pv.BookingId = b.Id AND pv.Type = 1 ORDER BY pv.Id DESC) pv_arrival
                OUTER APPLY (
                    SELECT TOP 1 
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi WHERE vi.EventId = je.Id AND vi.Label = 'Front') AS OnDemandFrontImageUrl,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi WHERE vi.EventId = je.Id AND vi.Label = 'Rear') AS OnDemandRearImageUrl,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi WHERE vi.EventId = je.Id AND vi.Label = 'LeftSide') AS OnDemandLeftSideImageUrl,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi WHERE vi.EventId = je.Id AND vi.Label = 'RightSide') AS OnDemandRightSideImageUrl,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi WHERE vi.EventId = je.Id AND vi.Label = 'Interior') AS OnDemandInteriorImageUrl,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi WHERE vi.EventId = je.Id AND vi.Label = 'Odometer') AS OnDemandOdometerImageUrl
                    FROM VehicleJourneyEvents je
                    WHERE je.VehicleId = b.VehicleId AND je.EventType = 'OnDemandUpdate'
                    ORDER BY je.CreatedAt DESC
                ) od
                OUTER APPLY (
                    SELECT TOP 1 
                        je.Description AS WeeklyUpdateDescription,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi WHERE vi.EventId = je.Id AND vi.Label = 'Front') AS WeeklyUpdateFrontImageUrl,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi WHERE vi.EventId = je.Id AND vi.Label = 'Rear') AS WeeklyUpdateRearImageUrl,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi WHERE vi.EventId = je.Id AND vi.Label = 'LeftSide') AS WeeklyUpdateLeftSideImageUrl,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi WHERE vi.EventId = je.Id AND vi.Label = 'RightSide') AS WeeklyUpdateRightSideImageUrl,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi WHERE vi.EventId = je.Id AND vi.Label = 'Interior') AS WeeklyUpdateInteriorImageUrl,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi WHERE vi.EventId = je.Id AND vi.Label = 'Odometer') AS WeeklyUpdateOdometerImageUrl
                    FROM VehicleJourneyEvents je
                    WHERE je.VehicleId = b.VehicleId AND je.EventType IN ('WeeklyUpdate', 'AdHocMaintenanceUpdate')
                    ORDER BY je.CreatedAt DESC
                ) wu
                WHERE  p.LotOwnerId = @LotOwnerId
                ORDER BY b.CreatedAt DESC";

            return await _db.QueryAsync<BookingDto>(sql, new { LotOwnerId = lotOwnerId });
        }

        public async Task<BookingDto?> GetLotOwnerBookingDetailAsync(long bookingId, long lotOwnerId)
        {
            const string sql = @"
                SELECT b.Id, b.VehicleId,
                       v.Brand AS VehicleBrand, v.Model AS VehicleModel,
                       v.RegistrationNo, v.VehicleRcUrl, v.OwnerIdProofUrl, v.VerificationStatus, v.HasServiceRecommendation, v.ManagerServiceRemarks,
                       (SELECT TOP 1 sr.Id FROM ServiceRequests sr WHERE sr.BookingId = b.Id AND sr.Status NOT IN ('Completed', 'Cancelled', 'Service Completed', 'Payment') ORDER BY sr.Id DESC) AS ActiveServiceRequestId,
                       CAST(CASE WHEN EXISTS (SELECT 1 FROM ServiceRequests sr WHERE sr.BookingId = b.Id AND sr.Status NOT IN ('Completed', 'Cancelled', 'Service Completed', 'Payment')) THEN 1 ELSE 0 END AS BIT) AS HasActiveServiceRequest,
                       (SELECT TOP 1 mt.CompletedAt FROM MaintenanceTasks mt WHERE mt.VehicleId = v.Id AND mt.Type = 1 ORDER BY mt.CompletedAt DESC) AS LastServiceReportDate,
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
                       pv_arrival.OdometerImageUrl AS ArrivalOdometerImageUrl, pv_arrival.ManagerRemarks AS ArrivalManagerRemarks,
                       od.OnDemandFrontImageUrl, od.OnDemandRearImageUrl, od.OnDemandLeftSideImageUrl, od.OnDemandRightSideImageUrl, od.OnDemandInteriorImageUrl, od.OnDemandOdometerImageUrl,
                       wu.WeeklyUpdateDescription, wu.WeeklyUpdateFrontImageUrl, wu.WeeklyUpdateRearImageUrl, wu.WeeklyUpdateLeftSideImageUrl, wu.WeeklyUpdateRightSideImageUrl, wu.WeeklyUpdateInteriorImageUrl, wu.WeeklyUpdateOdometerImageUrl
                FROM   Bookings b
                INNER JOIN Vehicles                 v ON b.VehicleId = v.Id
                INNER JOIN VehicleStorageProperties p ON b.PropertyId = p.Id
                LEFT  JOIN VehicleStorageSlots      s ON b.SlotId = s.Id
                OUTER APPLY (SELECT TOP 1 * FROM PickupRequests pr2 WHERE pr2.BookingId = b.Id ORDER BY pr2.Id DESC) pr
                LEFT  JOIN LotManagers              lm ON pr.ManagerId = lm.Id
                LEFT  JOIN Users                    mu ON lm.ManagerId = mu.Id
                OUTER APPLY (SELECT TOP 1 * FROM PickupVerifications pv WHERE pv.BookingId = b.Id AND pv.Type = 0 ORDER BY pv.Id DESC) pv_pickup
                OUTER APPLY (SELECT TOP 1 * FROM PickupVerifications pv WHERE pv.BookingId = b.Id AND pv.Type = 1 ORDER BY pv.Id DESC) pv_arrival
                OUTER APPLY (
                    SELECT TOP 1 
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi WHERE vi.EventId = je.Id AND vi.Label = 'Front') AS OnDemandFrontImageUrl,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi WHERE vi.EventId = je.Id AND vi.Label = 'Rear') AS OnDemandRearImageUrl,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi WHERE vi.EventId = je.Id AND vi.Label = 'LeftSide') AS OnDemandLeftSideImageUrl,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi WHERE vi.EventId = je.Id AND vi.Label = 'RightSide') AS OnDemandRightSideImageUrl,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi WHERE vi.EventId = je.Id AND vi.Label = 'Interior') AS OnDemandInteriorImageUrl,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi WHERE vi.EventId = je.Id AND vi.Label = 'Odometer') AS OnDemandOdometerImageUrl
                    FROM VehicleJourneyEvents je
                    WHERE je.VehicleId = b.VehicleId AND je.EventType = 'OnDemandUpdate'
                    ORDER BY je.CreatedAt DESC
                ) od
                OUTER APPLY (
                    SELECT TOP 1 
                        je.Description AS WeeklyUpdateDescription,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi WHERE vi.EventId = je.Id AND vi.Label = 'Front') AS WeeklyUpdateFrontImageUrl,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi WHERE vi.EventId = je.Id AND vi.Label = 'Rear') AS WeeklyUpdateRearImageUrl,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi WHERE vi.EventId = je.Id AND vi.Label = 'LeftSide') AS WeeklyUpdateLeftSideImageUrl,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi WHERE vi.EventId = je.Id AND vi.Label = 'RightSide') AS WeeklyUpdateRightSideImageUrl,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi WHERE vi.EventId = je.Id AND vi.Label = 'Interior') AS WeeklyUpdateInteriorImageUrl,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi WHERE vi.EventId = je.Id AND vi.Label = 'Odometer') AS WeeklyUpdateOdometerImageUrl
                    FROM VehicleJourneyEvents je
                    WHERE je.VehicleId = b.VehicleId AND je.EventType IN ('WeeklyUpdate', 'AdHocMaintenanceUpdate')
                    ORDER BY je.CreatedAt DESC
                ) wu
                WHERE  b.Id = @BookingId
                AND    p.LotOwnerId = @LotOwnerId";

            return await _db.QuerySingleOrDefaultAsync<BookingDto>(sql, new { BookingId = bookingId, LotOwnerId = lotOwnerId });
        }

        public async Task<BookingDto?> GetDetailAdminAsync(long bookingId)
        {
            const string sql = @"
                SELECT b.Id, b.VehicleId,
                       v.Brand AS VehicleBrand, v.Model AS VehicleModel,
                       v.RegistrationNo, v.VehicleRcUrl, v.OwnerIdProofUrl, v.HasServiceRecommendation, v.ManagerServiceRemarks,
                       (SELECT TOP 1 sr.Id FROM ServiceRequests sr WHERE sr.BookingId = b.Id AND sr.Status NOT IN ('Completed', 'Cancelled', 'Service Completed', 'Payment') ORDER BY sr.Id DESC) AS ActiveServiceRequestId,
                       CAST(CASE WHEN EXISTS (SELECT 1 FROM ServiceRequests sr WHERE sr.BookingId = b.Id AND sr.Status NOT IN ('Completed', 'Cancelled', 'Service Completed', 'Payment')) THEN 1 ELSE 0 END AS BIT) AS HasActiveServiceRequest,
                       (SELECT TOP 1 mt.CompletedAt FROM MaintenanceTasks mt WHERE mt.VehicleId = v.Id AND mt.Type = 1 ORDER BY mt.CompletedAt DESC) AS LastServiceReportDate,
                       (SELECT TOP 1 u.FullName FROM Users u WHERE u.Id = b.OwnerId) AS OwnerName,
                       (SELECT TOP 1 u2.FullName FROM Users u2 WHERE u2.Id = p.LotOwnerId) AS LotOwnerName,
                       (SELECT TOP 1 u2.PhoneNumber FROM Users u2 WHERE u2.Id = p.LotOwnerId) AS LotOwnerPhone,
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
                       pv_arrival.OdometerImageUrl AS ArrivalOdometerImageUrl, pv_arrival.ManagerRemarks AS ArrivalManagerRemarks,
                       od.OnDemandFrontImageUrl, od.OnDemandRearImageUrl, od.OnDemandLeftSideImageUrl, od.OnDemandRightSideImageUrl, od.OnDemandInteriorImageUrl, od.OnDemandOdometerImageUrl,
                       wu.WeeklyUpdateDescription, wu.WeeklyUpdateFrontImageUrl, wu.WeeklyUpdateRearImageUrl, wu.WeeklyUpdateLeftSideImageUrl, wu.WeeklyUpdateRightSideImageUrl, wu.WeeklyUpdateInteriorImageUrl, wu.WeeklyUpdateOdometerImageUrl
                FROM   Bookings b
                INNER JOIN Vehicles                 v ON b.VehicleId = v.Id
                INNER JOIN VehicleStorageProperties p ON b.PropertyId = p.Id
                LEFT  JOIN VehicleStorageSlots      s ON b.SlotId = s.Id
                OUTER APPLY (SELECT TOP 1 * FROM PickupRequests pr2 WHERE pr2.BookingId = b.Id ORDER BY pr2.Id DESC) pr
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
                       v.RegistrationNo, v.VehicleRcUrl, v.OwnerIdProofUrl, v.HasServiceRecommendation, v.ManagerServiceRemarks,
                       (SELECT TOP 1 sr.Id FROM ServiceRequests sr WHERE sr.BookingId = b.Id AND sr.Status NOT IN ('Completed', 'Cancelled', 'Service Completed', 'Payment') ORDER BY sr.Id DESC) AS ActiveServiceRequestId,
                       CAST(CASE WHEN EXISTS (SELECT 1 FROM ServiceRequests sr WHERE sr.BookingId = b.Id AND sr.Status NOT IN ('Completed', 'Cancelled', 'Service Completed', 'Payment')) THEN 1 ELSE 0 END AS BIT) AS HasActiveServiceRequest,
                       (SELECT TOP 1 u.FullName FROM Users u WHERE u.Id = b.OwnerId) AS OwnerName,
                       (SELECT TOP 1 u2.FullName FROM Users u2 WHERE u2.Id = p.LotOwnerId) AS LotOwnerName,
                       (SELECT TOP 1 u2.PhoneNumber FROM Users u2 WHERE u2.Id = p.LotOwnerId) AS LotOwnerPhone,
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
                       pv_arrival.OdometerImageUrl AS ArrivalOdometerImageUrl, pv_arrival.ManagerRemarks AS ArrivalManagerRemarks,
                       od.OnDemandFrontImageUrl, od.OnDemandRearImageUrl, od.OnDemandLeftSideImageUrl, od.OnDemandRightSideImageUrl, od.OnDemandInteriorImageUrl, od.OnDemandOdometerImageUrl,
                       wu.WeeklyUpdateDescription, wu.WeeklyUpdateFrontImageUrl, wu.WeeklyUpdateRearImageUrl, wu.WeeklyUpdateLeftSideImageUrl, wu.WeeklyUpdateRightSideImageUrl, wu.WeeklyUpdateInteriorImageUrl, wu.WeeklyUpdateOdometerImageUrl
                FROM   Bookings b
                INNER JOIN Vehicles                 v ON b.VehicleId = v.Id
                INNER JOIN VehicleStorageProperties p ON b.PropertyId = p.Id
                LEFT  JOIN VehicleStorageSlots      s ON b.SlotId = s.Id
                OUTER APPLY (SELECT TOP 1 * FROM PickupRequests pr2 WHERE pr2.BookingId = b.Id ORDER BY pr2.Id DESC) pr
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
                       v.RegistrationNo, v.VehicleRcUrl, v.OwnerIdProofUrl, v.HasServiceRecommendation, v.ManagerServiceRemarks,
                       (SELECT TOP 1 sr.Id FROM ServiceRequests sr WHERE sr.BookingId = b.Id AND sr.Status NOT IN ('Completed', 'Cancelled', 'Service Completed', 'Payment') ORDER BY sr.Id DESC) AS ActiveServiceRequestId,
                       CAST(CASE WHEN EXISTS (SELECT 1 FROM ServiceRequests sr WHERE sr.BookingId = b.Id AND sr.Status NOT IN ('Completed', 'Cancelled', 'Service Completed', 'Payment')) THEN 1 ELSE 0 END AS BIT) AS HasActiveServiceRequest,
                       (SELECT TOP 1 u.FullName FROM Users u WHERE u.Id = b.OwnerId) AS OwnerName,
                       (SELECT TOP 1 u2.FullName FROM Users u2 WHERE u2.Id = p.LotOwnerId) AS LotOwnerName,
                       (SELECT TOP 1 u2.PhoneNumber FROM Users u2 WHERE u2.Id = p.LotOwnerId) AS LotOwnerPhone,
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
                       pv_arrival.OdometerImageUrl AS ArrivalOdometerImageUrl, pv_arrival.ManagerRemarks AS ArrivalManagerRemarks,
                       od.OnDemandFrontImageUrl, od.OnDemandRearImageUrl, od.OnDemandLeftSideImageUrl, od.OnDemandRightSideImageUrl, od.OnDemandInteriorImageUrl, od.OnDemandOdometerImageUrl,
                       wu.WeeklyUpdateDescription, wu.WeeklyUpdateFrontImageUrl, wu.WeeklyUpdateRearImageUrl, wu.WeeklyUpdateLeftSideImageUrl, wu.WeeklyUpdateRightSideImageUrl, wu.WeeklyUpdateInteriorImageUrl, wu.WeeklyUpdateOdometerImageUrl
                FROM   Bookings b
                INNER JOIN Vehicles                 v ON b.VehicleId = v.Id
                INNER JOIN VehicleStorageProperties p ON b.PropertyId = p.Id
                LEFT  JOIN VehicleStorageSlots      s ON b.SlotId = s.Id
                OUTER APPLY (SELECT TOP 1 * FROM PickupRequests pr2 WHERE pr2.BookingId = b.Id ORDER BY pr2.Id DESC) pr
                LEFT  JOIN LotManagers              lm ON pr.ManagerId = lm.Id
                LEFT  JOIN Users                    mu ON lm.ManagerId = mu.Id
                OUTER APPLY (SELECT TOP 1 * FROM PickupVerifications pv WHERE pv.BookingId = b.Id AND pv.Type = 0 ORDER BY pv.Id DESC) pv_pickup
                OUTER APPLY (SELECT TOP 1 * FROM PickupVerifications pv WHERE pv.BookingId = b.Id AND pv.Type = 1 ORDER BY pv.Id DESC) pv_arrival
                ORDER BY b.CreatedAt DESC";

            return await _db.QueryAsync<BookingDto>(sql);
        }
        public async Task<IEnumerable<GD1.Application.Features.LotManager.Queries.ManagerVehicleDto>> GetLotOwnerVehiclesAsync(long lotOwnerId)
        {
            var sql = @"
                SELECT 
                    v.Id as VehicleId,
                    b.Id as BookingId,
                    v.Brand,
                    v.Model,
                    v.RegistrationNo,
                    o.FullName as OwnerName,
                    b.StartDate as StoredSince,
                    vi.ImageUrl,
                    CAST(ISNULL(pnd.HasPendingOnDemandRequest, 0) AS BIT) as HasPendingOnDemandRequest
                FROM VehicleStorageProperties p
                INNER JOIN Bookings b ON p.Id = b.PropertyId
                INNER JOIN Vehicles v ON b.VehicleId = v.Id
                INNER JOIN Users o ON b.OwnerId = o.Id
                OUTER APPLY (SELECT TOP 1 ImageUrl FROM VehicleImages WHERE VehicleId = v.Id AND EventId IS NULL) vi
                OUTER APPLY (SELECT TOP 1 1 as HasPendingOnDemandRequest FROM MaintenanceTasks mt2 WHERE mt2.VehicleId = v.Id AND mt2.Type = 0 AND mt2.Status = 0) pnd
                WHERE p.LotOwnerId = @OwnerId AND p.IsDeleted = 0 AND b.Status IN (2, 3) -- InLot, Completed
                ORDER BY b.StartDate DESC
            ";
            return await _db.QueryAsync<GD1.Application.Features.LotManager.Queries.ManagerVehicleDto>(sql, new { OwnerId = lotOwnerId });
        }

        public async Task<GD1.Application.Features.LotManager.Queries.ManagerVehicleDetailDto> GetLotOwnerVehicleDetailAsync(long lotOwnerId, long vehicleId)
        {
            var sql = @"
                SELECT 
                    v.Id as VehicleId,
                    b.Id as BookingId,
                    v.Brand,
                    v.Model,
                    v.RegistrationNo,
                    v.Category,
                    v.VerificationStatus, v.HasServiceRecommendation, v.ManagerServiceRemarks,
                    o.Id as OwnerId,
                    o.FullName as OwnerName,
                    o.PhoneNumber as OwnerPhone,
                    o.Email as OwnerEmail,
                    lo.Id as LotOwnerId,
                    lo.FullName as LotOwnerName,
                    lo.PhoneNumber as LotOwnerPhone,
                    b.StartDate as StoredSince,
                    b.PricePerDay,
                    vi.ImageUrl,
                    od.LastOnDemandImageDate,
                    srr.LastServiceReportDate,
                    CAST(ISNULL(pnd.HasPendingOnDemandRequest, 0) AS BIT) as HasPendingOnDemandRequest,
                    odi.OnDemandFrontImageUrl,
                    odi.OnDemandRearImageUrl,
                    odi.OnDemandLeftSideImageUrl,
                    odi.OnDemandRightSideImageUrl,
                    odi.OnDemandInteriorImageUrl,
                    odi.OnDemandOdometerImageUrl,
                    wui.WeeklyUpdateDescription,
                    wui.LastWeeklyUpdateDate,
                    wui.WeeklyUpdateFrontImageUrl,
                    wui.WeeklyUpdateRearImageUrl,
                    wui.WeeklyUpdateLeftSideImageUrl,
                    wui.WeeklyUpdateRightSideImageUrl,
                    wui.WeeklyUpdateInteriorImageUrl,
                    wui.WeeklyUpdateOdometerImageUrl
                FROM VehicleStorageProperties p
                INNER JOIN Bookings b ON p.Id = b.PropertyId
                INNER JOIN Vehicles v ON b.VehicleId = v.Id
                INNER JOIN Users o ON b.OwnerId = o.Id
                INNER JOIN Users lo ON p.LotOwnerId = lo.Id
                OUTER APPLY (SELECT TOP 1 ImageUrl FROM VehicleImages WHERE VehicleId = v.Id AND EventId IS NULL) vi
                OUTER APPLY (SELECT TOP 1 mt.CompletedAt as LastOnDemandImageDate FROM MaintenanceTasks mt WHERE mt.VehicleId = v.Id AND mt.Type = 0 AND mt.Status = 1 ORDER BY mt.CompletedAt DESC) od
                OUTER APPLY (SELECT TOP 1 sr.UpdatedAt as LastServiceReportDate FROM ServiceRequests sr INNER JOIN Bookings b2 ON sr.BookingId = b2.Id WHERE b2.VehicleId = v.Id AND sr.IsCompleted = 1 ORDER BY sr.UpdatedAt DESC) srr
                OUTER APPLY (SELECT TOP 1 1 as HasPendingOnDemandRequest FROM MaintenanceTasks mt2 WHERE mt2.VehicleId = v.Id AND mt2.Type = 0 AND mt2.Status = 0) pnd
                OUTER APPLY (
                    SELECT TOP 1 
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi2 WHERE vi2.EventId = je.Id AND vi2.Label = 'Front') AS OnDemandFrontImageUrl,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi2 WHERE vi2.EventId = je.Id AND vi2.Label = 'Rear') AS OnDemandRearImageUrl,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi2 WHERE vi2.EventId = je.Id AND vi2.Label = 'LeftSide') AS OnDemandLeftSideImageUrl,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi2 WHERE vi2.EventId = je.Id AND vi2.Label = 'RightSide') AS OnDemandRightSideImageUrl,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi2 WHERE vi2.EventId = je.Id AND vi2.Label = 'Interior') AS OnDemandInteriorImageUrl,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi2 WHERE vi2.EventId = je.Id AND vi2.Label = 'Odometer') AS OnDemandOdometerImageUrl
                    FROM VehicleJourneyEvents je
                    WHERE je.VehicleId = v.Id AND je.EventType = 'OnDemandUpdate'
                    ORDER BY je.CreatedAt DESC
                ) odi
                OUTER APPLY (
                    SELECT TOP 1 
                        je.CreatedAt AS LastWeeklyUpdateDate,
                        je.Description AS WeeklyUpdateDescription,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi2 WHERE vi2.EventId = je.Id AND vi2.Label = 'Front') AS WeeklyUpdateFrontImageUrl,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi2 WHERE vi2.EventId = je.Id AND vi2.Label = 'Rear') AS WeeklyUpdateRearImageUrl,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi2 WHERE vi2.EventId = je.Id AND vi2.Label = 'LeftSide') AS WeeklyUpdateLeftSideImageUrl,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi2 WHERE vi2.EventId = je.Id AND vi2.Label = 'RightSide') AS WeeklyUpdateRightSideImageUrl,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi2 WHERE vi2.EventId = je.Id AND vi2.Label = 'Interior') AS WeeklyUpdateInteriorImageUrl,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi2 WHERE vi2.EventId = je.Id AND vi2.Label = 'Odometer') AS WeeklyUpdateOdometerImageUrl
                    FROM VehicleJourneyEvents je
                    WHERE je.VehicleId = v.Id AND je.EventType IN ('WeeklyUpdate', 'AdHocMaintenanceUpdate')
                    ORDER BY je.CreatedAt DESC
                ) wui
                WHERE p.LotOwnerId = @OwnerId AND v.Id = @VehicleId AND p.IsDeleted = 0 AND b.Status IN (2, 3)
            ";

            var data = await _db.QueryFirstOrDefaultAsync<GD1.Application.Features.LotManager.Queries.ManagerVehicleDetailDto>(sql, new { OwnerId = lotOwnerId, VehicleId = vehicleId });
            
            return data;
        }
    }
}

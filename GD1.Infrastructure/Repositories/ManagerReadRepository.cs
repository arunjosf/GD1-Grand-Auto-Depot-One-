using Dapper;
using GD1.Application.Features.LotManager.Queries;
using GD1.Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace GD1.Infrastructure.Repositories
{
    public class ManagerReadRepository : IManagerReadRepository
    {
        private readonly IDbConnection _db;

        public ManagerReadRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<ManagerDashboardMetricsDto> GetDashboardMetricsAsync(long managerId)
        {
            var metrics = new ManagerDashboardMetricsDto();
            
            // 1. Total Vehicles
            var totalVehiclesSql = @"
                SELECT COUNT(DISTINCT b.VehicleId)
                FROM LotManagers lm
                INNER JOIN Bookings b ON lm.PropertyId = b.PropertyId
                WHERE lm.ManagerId = @ManagerId AND lm.IsActive = 1 AND b.Status IN (2, 3) -- InLot, Completed
            ";
            metrics.TotalVehicles = await _db.ExecuteScalarAsync<int>(totalVehiclesSql, new { ManagerId = managerId });

            // 2. Pending Pickups
            var pendingPickupsSql = @"
                SELECT COUNT(pr.Id)
                FROM PickupRequests pr
                INNER JOIN LotManagers lm ON pr.ManagerId = lm.Id
                WHERE lm.ManagerId = @ManagerId AND pr.Status NOT IN (10, 4) -- Stored, Declined
            ";
            metrics.PendingPickupsCount = await _db.ExecuteScalarAsync<int>(pendingPickupsSql, new { ManagerId = managerId });

            // 3. Upcoming Services
            var upcomingServicesSql = @"
                SELECT COUNT(sr.Id)
                FROM LotManagers lm
                INNER JOIN Bookings b2 ON lm.PropertyId = b2.PropertyId INNER JOIN ServiceRequests sr ON b2.Id = sr.BookingId
                WHERE lm.ManagerId = @ManagerId AND lm.IsActive = 1 AND sr.Status NOT IN ('Service Completed', 'Payment', 'Cancelled')
            ";
            metrics.UpcomingServicesCount = await _db.ExecuteScalarAsync<int>(upcomingServicesSql, new { ManagerId = managerId });

            // 4. Pending Tasks
            var pendingTasksSql = @"
                SELECT mt.Type, COUNT(mt.Id) as Count
                FROM MaintenanceTasks mt
                INNER JOIN LotManagers lm ON mt.ManagerId = lm.Id
                WHERE lm.ManagerId = @ManagerId AND mt.Status = 0 -- Pending
                GROUP BY mt.Type
            ";
            var tasks = await _db.QueryAsync(pendingTasksSql, new { ManagerId = managerId });
            foreach (var t in tasks)
            {
                var type = t.Type;
                if (type != null)
                {
                    string typeStr = type.ToString();
                    if (typeStr == "0" || typeStr == "OnDemandImage") metrics.PendingOnDemandCount = (int)t.Count;
                    else if (typeStr == "1" || typeStr == "WeeklyConditionCheck") metrics.PendingWeeklyCount = (int)t.Count;
                }
            }

            // 5. Performance Graph (Last 7 days)
            var graphSql = @"
                WITH DateCTE AS (
                    SELECT CAST(GETUTCDATE() AS DATE) AS Date
                    UNION ALL
                    SELECT DATEADD(day, -1, Date)
                    FROM DateCTE
                    WHERE Date > DATEADD(day, -6, CAST(GETUTCDATE() AS DATE))
                )
                SELECT 
                    FORMAT(d.Date, 'MMM dd') as Date,
                    (SELECT COUNT(*) FROM PickupRequests pr INNER JOIN LotManagers lm ON pr.ManagerId = lm.Id WHERE lm.ManagerId = @ManagerId AND pr.Status IN (6, 7, 8, 9, 10) AND CAST(pr.UpdatedAt AS DATE) = d.Date) as PickupsDone,
                    (SELECT COUNT(*) FROM MaintenanceTasks mt INNER JOIN LotManagers lm ON mt.ManagerId = lm.Id WHERE lm.ManagerId = @ManagerId AND mt.Type = 0 AND mt.Status = 1 AND CAST(mt.UpdatedAt AS DATE) = d.Date) as OnDemandImagesDone,
                    (SELECT COUNT(*) FROM MaintenanceTasks mt INNER JOIN LotManagers lm ON mt.ManagerId = lm.Id WHERE lm.ManagerId = @ManagerId AND mt.Type = 1 AND mt.Status = 1 AND CAST(mt.UpdatedAt AS DATE) = d.Date) as WeeklySubmissionsDone
                FROM DateCTE d
                ORDER BY d.Date ASC
            ";
            metrics.PerformanceGraphData = (await _db.QueryAsync<PerformanceGraphItemDto>(graphSql, new { ManagerId = managerId })).ToList();

            return metrics;
        }

        public async Task<IEnumerable<ManagerPickupDto>> GetPickupsAsync(long managerId, bool isCompleted)
        {
            string statusFilter = isCompleted ? "IN (10, 4)" : "NOT IN (10, 4)";
            var sql = $@"
                SELECT 
                    pr.Id as PickupRequestId,
                    b.Id as BookingId,
                    o.FullName as CustomerName,
                    v.Brand as VehicleBrand,
                    v.Model as VehicleModel,
                    v.RegistrationNo,
                    pr.RequestedPickupTime,
                    pr.Status,
                    vi.ImageUrl as VehicleImage,
                    b.PickupAddress as PickupAddress
                FROM PickupRequests pr
                INNER JOIN Bookings b ON pr.BookingId = b.Id
                INNER JOIN Vehicles v ON b.VehicleId = v.Id
                INNER JOIN Users o ON b.OwnerId = o.Id
                INNER JOIN LotManagers lm ON pr.ManagerId = lm.Id
                OUTER APPLY (SELECT TOP 1 ImageUrl FROM VehicleImages WHERE VehicleId = v.Id AND UploadedBy = 'Owner' AND EventId IS NULL ORDER BY Id ASC) vi
                WHERE lm.ManagerId = @ManagerId AND pr.Status {statusFilter}
                ORDER BY pr.RequestedPickupTime ASC
            ";
            return await _db.QueryAsync<ManagerPickupDto>(sql, new { ManagerId = managerId });
        }

        public async Task<IEnumerable<ManagerVehicleDto>> GetVehiclesAsync(long managerId)
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
                    b.EndDate,
                    b.Status as BookingStatus,
                    vi.ImageUrl,
                    CAST(ISNULL(pnd.HasPendingOnDemandRequest, 0) AS BIT) as HasPendingOnDemandRequest
                FROM LotManagers lm
                INNER JOIN Bookings b ON lm.PropertyId = b.PropertyId
                INNER JOIN Vehicles v ON b.VehicleId = v.Id
                INNER JOIN Users o ON b.OwnerId = o.Id
                OUTER APPLY (SELECT TOP 1 ImageUrl FROM VehicleImages WHERE VehicleId = v.Id AND UploadedBy = 'Owner' AND EventId IS NULL ORDER BY Id ASC) vi
                OUTER APPLY (SELECT TOP 1 1 as HasPendingOnDemandRequest FROM MaintenanceTasks mt2 WHERE mt2.BookingId = b.Id AND mt2.Type = 0 AND mt2.Status = 0) pnd
                WHERE lm.ManagerId = @ManagerId AND lm.IsActive = 1 AND b.Status IN (2, 3) -- InLot, Completed
                ORDER BY b.StartDate DESC
            ";
            return await _db.QueryAsync<ManagerVehicleDto>(sql, new { ManagerId = managerId });
        }

        public async Task<ManagerVehicleDetailDto> GetVehicleDetailAsync(long managerId, long vehicleId, long? bookingId = null)
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
                    b.EndDate as EndDate,
                    b.Status as BookingStatus,
                    b.PricePerDay,
                    vi.ImageUrl,
                    od.LastOnDemandImageDate,
                    srr.LastServiceReportDate,
                    srr.LastServiceCost,
                    srr.LastServiceNotes,
                    srr.LastServiceCenterName,
                    srr.LastServiceBillUrl,
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
                FROM LotManagers lm
                INNER JOIN VehicleStorageProperties p ON lm.PropertyId = p.Id
                INNER JOIN Users lo ON p.LotOwnerId = lo.Id
                INNER JOIN (
                    SELECT TOP 1 * FROM Bookings b2 
                    WHERE b2.VehicleId = @VehicleId AND b2.Status IN (2, 3) 
                      AND (@BookingId IS NULL OR b2.Id = @BookingId)
                    ORDER BY b2.CreatedAt DESC
                ) b ON b.PropertyId = lm.PropertyId
                INNER JOIN Vehicles v ON b.VehicleId = v.Id
                INNER JOIN Users o ON b.OwnerId = o.Id
                OUTER APPLY (SELECT TOP 1 ImageUrl FROM VehicleImages WHERE VehicleId = v.Id AND UploadedBy = 'Owner' AND EventId IS NULL ORDER BY Id ASC) vi
                OUTER APPLY (SELECT TOP 1 mt.CompletedAt as LastOnDemandImageDate FROM MaintenanceTasks mt WHERE mt.BookingId = b.Id AND mt.Type = 0 AND mt.Status = 1 ORDER BY mt.CompletedAt DESC) od
                OUTER APPLY (
                    SELECT TOP 1 sr.UpdatedAt as LastServiceReportDate, sr.ServiceCost as LastServiceCost, sr.CompletionNotes as LastServiceNotes, c.Name as LastServiceCenterName, sr.BillUrl as LastServiceBillUrl
                    FROM ServiceRequests sr 
                    INNER JOIN ServiceCenters c ON sr.ServiceCenterId = c.Id
                    WHERE sr.BookingId = b.Id AND (sr.IsCompleted = 1 OR sr.Status IN ('Service Completed', 'Completed', 'Payment Completed')) 
                    ORDER BY sr.UpdatedAt DESC
                ) srr
                OUTER APPLY (SELECT TOP 1 1 as HasPendingOnDemandRequest FROM MaintenanceTasks mt2 WHERE mt2.BookingId = b.Id AND mt2.Type = 0 AND mt2.Status = 0) pnd
                OUTER APPLY (
                    SELECT TOP 1 
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi2 WHERE vi2.EventId = je.Id AND vi2.Label = 'Front') AS OnDemandFrontImageUrl,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi2 WHERE vi2.EventId = je.Id AND vi2.Label = 'Rear') AS OnDemandRearImageUrl,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi2 WHERE vi2.EventId = je.Id AND vi2.Label = 'LeftSide') AS OnDemandLeftSideImageUrl,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi2 WHERE vi2.EventId = je.Id AND vi2.Label = 'RightSide') AS OnDemandRightSideImageUrl,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi2 WHERE vi2.EventId = je.Id AND vi2.Label = 'Interior') AS OnDemandInteriorImageUrl,
                        (SELECT TOP 1 ImageUrl FROM VehicleImages vi2 WHERE vi2.EventId = je.Id AND vi2.Label = 'Odometer') AS OnDemandOdometerImageUrl
                    FROM VehicleJourneyEvents je
                    WHERE je.BookingId = b.Id AND je.EventType = 'OnDemandUpdate'
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
                    WHERE je.BookingId = b.Id AND je.EventType IN ('WeeklyUpdate', 'AdHocMaintenanceUpdate', 'Weekly Condition Submitted')
                    ORDER BY je.CreatedAt DESC
                ) wui
                WHERE lm.ManagerId = @ManagerId AND lm.IsActive = 1 AND v.Id = @VehicleId
            ";
            return await _db.QueryFirstOrDefaultAsync<ManagerVehicleDetailDto>(sql, new { ManagerId = managerId, VehicleId = vehicleId, BookingId = bookingId });
        }
    }
}

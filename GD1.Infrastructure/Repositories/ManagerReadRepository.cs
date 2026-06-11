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
                INNER JOIN ServiceRequests sr ON lm.PropertyId = sr.PropertyId
                WHERE lm.ManagerId = @ManagerId AND lm.IsActive = 1 AND sr.Status IN (0, 1, 2) -- Requested, Scheduled, ManagerAssigned
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
                if (t.Type == 0) metrics.PendingWeeklyCount = t.Count;
                if (t.Type == 1) metrics.PendingOnDemandCount = t.Count;
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
                    (SELECT COUNT(*) FROM MaintenanceTasks mt INNER JOIN LotManagers lm ON mt.ManagerId = lm.Id WHERE lm.ManagerId = @ManagerId AND mt.Type = 1 AND mt.Status = 1 AND CAST(mt.UpdatedAt AS DATE) = d.Date) as OnDemandImagesDone,
                    (SELECT COUNT(*) FROM MaintenanceTasks mt INNER JOIN LotManagers lm ON mt.ManagerId = lm.Id WHERE lm.ManagerId = @ManagerId AND mt.Type = 0 AND mt.Status = 1 AND CAST(mt.UpdatedAt AS DATE) = d.Date) as WeeklySubmissionsDone
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
                OUTER APPLY (SELECT TOP 1 ImageUrl FROM VehicleImages WHERE VehicleId = v.Id AND EventId IS NULL) vi
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
                    vi.ImageUrl
                FROM LotManagers lm
                INNER JOIN Bookings b ON lm.PropertyId = b.PropertyId
                INNER JOIN Vehicles v ON b.VehicleId = v.Id
                INNER JOIN Users o ON b.OwnerId = o.Id
                OUTER APPLY (SELECT TOP 1 ImageUrl FROM VehicleImages WHERE VehicleId = v.Id AND EventId IS NULL) vi
                WHERE lm.ManagerId = @ManagerId AND lm.IsActive = 1 AND b.Status IN (2, 3) -- InLot, Completed
                ORDER BY b.StartDate DESC
            ";
            return await _db.QueryAsync<ManagerVehicleDto>(sql, new { ManagerId = managerId });
        }

        public async Task<ManagerVehicleDetailDto> GetVehicleDetailAsync(long managerId, long vehicleId)
        {
            var sql = @"
                SELECT 
                    v.Id as VehicleId,
                    b.Id as BookingId,
                    v.Brand,
                    v.Model,
                    v.RegistrationNo,
                    v.Category,
                    v.VerificationStatus,
                    o.FullName as OwnerName,
                    o.PhoneNumber as OwnerPhone,
                    o.Email as OwnerEmail,
                    b.StartDate as StoredSince,
                    b.PricePerDay,
                    vi.ImageUrl
                FROM LotManagers lm
                INNER JOIN Bookings b ON lm.PropertyId = b.PropertyId
                INNER JOIN Vehicles v ON b.VehicleId = v.Id
                INNER JOIN Users o ON b.OwnerId = o.Id
                OUTER APPLY (SELECT TOP 1 ImageUrl FROM VehicleImages WHERE VehicleId = v.Id AND EventId IS NULL) vi
                WHERE lm.ManagerId = @ManagerId AND lm.IsActive = 1 AND v.Id = @VehicleId
            ";
            return await _db.QueryFirstOrDefaultAsync<ManagerVehicleDetailDto>(sql, new { ManagerId = managerId, VehicleId = vehicleId });
        }
    }
}

using Dapper;
using GD1.Application.Features.Pickup.Queries;
using GD1.Application.Interfaces.Repositories;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace GD1.Infrastructure.Repositories
{
    public class PickupReadRepository : IPickupReadRepository
    {
        private readonly IDbConnection _db;

        public PickupReadRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<IEnumerable<PickupRequestDto>> GetPropertyPickupsAsync(long propertyId, long? managerId)
        {
            var sql = @"
                SELECT 
                    pr.Id as PickupRequestId,
                    b.Id as BookingId,
                    pr.RequestedPickupTime,
                    pr.Status,
                    
                    v.Brand as VehicleBrand,
                    v.Model as VehicleModel,
                    v.RegistrationNo,
                    o.FullName as CustomerName,
                    o.Email as CustomerEmail,
                    o.PhoneNumber as CustomerPhone,
                    b.PickupAddress,
                    b.PickupPincode,
                    b.PickupLatitude,
                    b.PickupLongitude,
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
                FROM PickupRequests pr
                INNER JOIN Bookings b ON pr.BookingId = b.Id
                INNER JOIN Vehicles v ON b.VehicleId = v.Id
                INNER JOIN Users o ON b.OwnerId = o.Id
                LEFT JOIN PickupVerifications pv_pickup ON pv_pickup.BookingId = b.Id AND pv_pickup.Type = 0
                LEFT JOIN PickupVerifications pv_arrival ON pv_arrival.BookingId = b.Id AND pv_arrival.Type = 1
                WHERE b.PropertyId = @PropertyId";

            if (managerId.HasValue)
            {
                sql += " AND pr.ManagerId = @ManagerId";
            }

            sql += " ORDER BY pr.RequestedPickupTime DESC";

            return await _db.QueryAsync<PickupRequestDto>(sql, new { PropertyId = propertyId, ManagerId = managerId });
        }

        public async Task<IEnumerable<PickupRequestDto>> GetMyAssignmentsAsync(long managerUserId)
        {
            var sql = @"
                SELECT 
                    pr.Id as PickupRequestId,
                    b.Id as BookingId,
                    pr.RequestedPickupTime,
                    pr.Status,
                    
                    v.Brand as VehicleBrand,
                    v.Model as VehicleModel,
                    v.RegistrationNo,
                    o.FullName as CustomerName,
                    o.Email as CustomerEmail,
                    o.PhoneNumber as CustomerPhone,
                    b.PickupAddress,
                    b.PickupPincode,
                    b.PickupLatitude,
                    b.PickupLongitude,
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
                FROM PickupRequests pr
                INNER JOIN Bookings b ON pr.BookingId = b.Id
                INNER JOIN Vehicles v ON b.VehicleId = v.Id
                INNER JOIN Users o ON b.OwnerId = o.Id
                INNER JOIN LotManagers lm ON pr.ManagerId = lm.Id
                LEFT JOIN PickupVerifications pv_pickup ON pv_pickup.BookingId = b.Id AND pv_pickup.Type = 0
                LEFT JOIN PickupVerifications pv_arrival ON pv_arrival.BookingId = b.Id AND pv_arrival.Type = 1
                WHERE lm.ManagerId = @ManagerUserId AND pr.Status != 9
                ORDER BY pr.RequestedPickupTime DESC";

            return await _db.QueryAsync<PickupRequestDto>(sql, new { ManagerUserId = managerUserId });
        }
    }
}

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

        public async Task<IEnumerable<AssignedPickupDto>> GetAssignedPickupsAsync(long managerId)
        {
            const string sql = @"
                SELECT 
                    pr.Id AS PickupRequestId,
                    pr.BookingId,
                    pr.RequestedPickupTime,
                    CASE pr.Status
                        WHEN 0 THEN 'Requested'
                        WHEN 1 THEN 'Assigned'
                        WHEN 2 THEN 'ManagerScheduled'
                        WHEN 3 THEN 'Approved'
                        WHEN 4 THEN 'OtpSent'
                        WHEN 5 THEN 'Verified'
                        WHEN 6 THEN 'VehiclePicked'
                        ELSE 'Unknown'
                    END AS Status,
                    v.Brand AS VehicleBrand,
                    v.Model AS VehicleModel,
                    v.RegistrationNo,
                    u.FullName AS CustomerName,
                    u.Email AS CustomerEmail,
                    u.PhoneNumber AS CustomerPhone,
                    b.PickupAddress,
                    b.PickupPincode,
                    b.PickupLatitude,
                    b.PickupLongitude
                FROM PickupRequests pr
                INNER JOIN Bookings b ON pr.BookingId = b.Id
                INNER JOIN Vehicles v ON b.VehicleId = v.Id
                INNER JOIN Users u ON b.OwnerId = u.Id
                WHERE pr.ManagerId = @ManagerId
                ORDER BY pr.RequestedPickupTime ASC";

            return await _db.QueryAsync<AssignedPickupDto>(sql, new { ManagerId = managerId });
        }
    }
}

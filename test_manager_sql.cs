using System;
using System.Data.SqlClient;
using Dapper;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        string connectionString = ""Server=(localdb)\\mssqllocaldb;Database=GD1_Db;Trusted_Connection=True;MultipleActiveResultSets=true"";
        string sql = @""
                  SELECT 
                      b.Id AS BookingId,
                      v.Brand AS VehicleBrand, v.Model AS VehicleModel, v.RegistrationNo,
                      u.FullName AS CustomerName, u.PhoneNumber AS CustomerPhone, u.Email AS CustomerEmail,
                      b.StartDate, b.EndDate,
                      COALESCE(s.SlotNumber, 'Unassigned') AS SlotName,
                      p.Name AS PropertyName,
                      v.OwnerIdProofUrl, v.VehicleRcUrl,
                      CASE b.Status WHEN 1 THEN 'Confirmed' WHEN 2 THEN 'InLot' WHEN 3 THEN 'Completed' ELSE 'Unknown' END AS Status,
                      pv.FrontImageUrl, pv.RearImageUrl, pv.LeftSideImageUrl, pv.RightSideImageUrl, pv.InteriorImageUrl, pv.OdometerImageUrl,
                      pv.ManagerRemarks, pv.VerifiedAt
                  FROM Bookings b
                  INNER JOIN Vehicles v ON b.VehicleId = v.Id
                  INNER JOIN Users u ON b.OwnerId = u.Id
                  INNER JOIN VehicleStorageProperties p ON b.PropertyId = p.Id
                  LEFT JOIN VehicleStorageSlots s ON b.SlotId = s.Id
                  LEFT JOIN PickupVerifications pv ON b.Id = pv.BookingId AND pv.Type = 1 -- LotArrival
                  INNER JOIN LotManagers lm ON b.PropertyId = lm.PropertyId AND lm.ManagerId = @UserId AND lm.IsActive = 1
                  OUTER APPLY (SELECT TOP 1 ImageUrl FROM VehicleImages WHERE VehicleId = v.Id AND UploadedBy = 'Owner' AND EventId IS NULL ORDER BY Id ASC) vi
                  WHERE b.Id = @BookingId 
        "";
        
        try {
            using (var db = new SqlConnection(connectionString))
            {
                var result = await db.QueryFirstOrDefaultAsync<dynamic>(sql, new { BookingId = 1, UserId = 1 });
                Console.WriteLine(""Success!"");
            }
        } catch (Exception ex) {
            Console.WriteLine(ex.Message);
        }
    }
}

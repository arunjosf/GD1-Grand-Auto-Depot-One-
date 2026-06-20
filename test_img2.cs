using System;
using System.Data.SqlClient;

class Program
{
    static void Main()
    {
        string connectionString = "Server=.\\SQLEXPRESS;Database=GD1_DB;Trusted_Connection=True;TrustServerCertificate=True;";
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();
            SqlCommand command = new SqlCommand(@"
                SELECT pv.Id, pv.ManagerId, pv.SelfieUrl, 
                       lm.ManagerId as UserId, 
                       u.AvatarUrl, u.FullName 
                FROM PickupVerifications pv
                LEFT JOIN LotManagers lm ON pv.ManagerId = lm.Id
                LEFT JOIN Users u ON lm.ManagerId = u.Id", connection);
                
            using (SqlDataReader reader = command.ExecuteReader())
            {
                Console.WriteLine("Verifications:");
                while (reader.Read())
                {
                    Console.WriteLine($"PV_Id: {reader["Id"]}, PV_ManagerId: {reader["ManagerId"]}, SelfieUrl: {reader["SelfieUrl"]}, UserId: {reader["UserId"]}, AvatarUrl: {reader["AvatarUrl"]}, FullName: {reader["FullName"]}");
                }
            }
        }
    }
}

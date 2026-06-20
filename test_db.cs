using System;
using System.Data.SqlClient;

class Program
{
    static void Main()
    {
        string connectionString = "Server=(localdb)\\mssqllocaldb;Database=GD1_DB;Trusted_Connection=True;";
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();
            SqlCommand command = new SqlCommand("SELECT Id, BookingId, ManagerId, Type FROM PickupVerifications", connection);
            using (SqlDataReader reader = command.ExecuteReader())
            {
                Console.WriteLine("PickupVerifications:");
                while (reader.Read())
                {
                    Console.WriteLine($"Id: {reader["Id"]}, BookingId: {reader["BookingId"]}, ManagerId: {reader["ManagerId"]}, Type: {reader["Type"]}");
                }
            }

            command = new SqlCommand("SELECT Id, EventType, BookingId FROM VehicleJourneyEvents", connection);
            using (SqlDataReader reader = command.ExecuteReader())
            {
                Console.WriteLine("\nVehicleJourneyEvents:");
                while (reader.Read())
                {
                    Console.WriteLine($"Id: {reader["Id"]}, EventType: {reader["EventType"]}, BookingId: {reader["BookingId"]}");
                }
            }
        }
    }
}

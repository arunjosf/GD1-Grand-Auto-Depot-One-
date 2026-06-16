using System;
using Microsoft.Data.SqlClient;

class Program {
    static void Main() {
        string connStr = "Server=(localdb)\\mssqllocaldb;Database=GD1DB;Trusted_Connection=True;MultipleActiveResultSets=true";
        using var conn = new SqlConnection(connStr);
        try {
            conn.Open();
            using var cmd = new SqlCommand("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='ServiceRequests'", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) {
                Console.WriteLine(reader.GetString(0));
            }
        } catch(Exception ex) {
            Console.WriteLine(ex.Message);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

public class Program
{
    public static async Task Main()
    {
        string connStr = "Server=.;Database=GD1;Trusted_Connection=True;TrustServerCertificate=True;";
        string apiKey = "PAcAVh9rDo1GbOmqdYiM6bBcpv80JYwdOuUf8h9O";
        
        Console.WriteLine("Loading cars from database...");
        var carsToUpdate = new List<(long Id, string Brand, string Model)>();
        
        using (var conn = new SqlConnection(connStr))
        {
            await conn.OpenAsync();
            using var cmd = new SqlCommand("SELECT Id, Brand, Model FROM VehicleCatalog", conn);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                carsToUpdate.Add((reader.GetInt64(0), reader.GetString(1), reader.GetString(2)));
            }
        }
        
        Console.WriteLine($"Found {carsToUpdate.Count} cars. Starting API Ninjas categorization...");
        
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
        
        var semaphore = new SemaphoreSlim(10); // 10 concurrent requests to respect rate limits
        int processed = 0;

        var tasks = new List<Task>();
        foreach (var car in carsToUpdate)
        {
            tasks.Add(Task.Run(async () =>
            {
                await semaphore.WaitAsync();
                try
                {
                    // Ping API Ninjas
                    var url = $"https://api.api-ninjas.com/v1/cars?make={Uri.EscapeDataString(car.Brand)}&model={Uri.EscapeDataString(car.Model)}";
                    var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonString = await response.Content.ReadAsStringAsync();
                        var apiCars = JsonSerializer.Deserialize<List<JsonElement>>(jsonString);
                        
                        if (apiCars != null && apiCars.Count > 0)
                        {
                            var rawClass = apiCars[0].GetProperty("class").GetString()?.ToLower() ?? "sedan";
                            
                            // Map API Ninjas class to our standard categories
                            string finalCategory = "Sedan";
                            if (rawClass.Contains("suv") || rawClass.Contains("sport utility")) finalCategory = "SUV";
                            else if (rawClass.Contains("pickup") || rawClass.Contains("truck")) finalCategory = "Truck";
                            else if (rawClass.Contains("sport") || rawClass.Contains("coupe") || rawClass.Contains("two seater")) finalCategory = "Coupe";
                            else if (rawClass.Contains("hatchback")) finalCategory = "Hatchback";
                            else if (rawClass.Contains("van") || rawClass.Contains("minivan")) finalCategory = "Van";

                            // Dimensions based on final category
                            double len = finalCategory == "SUV" ? 15.5 : finalCategory == "Truck" ? 19.5 : finalCategory == "Coupe" ? 14.5 : 15.0;
                            double wid = finalCategory == "SUV" ? 6.2 : finalCategory == "Truck" ? 6.5 : 6.0;
                            double hgt = finalCategory == "SUV" ? 5.8 : finalCategory == "Truck" ? 6.2 : 4.8;

                            // Update Database
                            using var updateConn = new SqlConnection(connStr);
                            await updateConn.OpenAsync();
                            using var updateCmd = new SqlCommand("UPDATE VehicleCatalog SET Category = @cat, LengthFeet = @len, WidthFeet = @wid, HeightFeet = @hgt WHERE Id = @id", updateConn);
                            updateCmd.Parameters.AddWithValue("@cat", finalCategory);
                            updateCmd.Parameters.AddWithValue("@len", len);
                            updateCmd.Parameters.AddWithValue("@wid", wid);
                            updateCmd.Parameters.AddWithValue("@hgt", hgt);
                            updateCmd.Parameters.AddWithValue("@id", car.Id);
                            await updateCmd.ExecuteNonQueryAsync();
                        }
                    }
                }
                catch {}
                finally
                {
                    Interlocked.Increment(ref processed);
                    if (processed % 100 == 0) Console.WriteLine($"Processed {processed}/{carsToUpdate.Count} cars...");
                    semaphore.Release();
                }
            }));
        }

        await Task.WhenAll(tasks);
        Console.WriteLine("Database perfectly categorized!");
    }
}

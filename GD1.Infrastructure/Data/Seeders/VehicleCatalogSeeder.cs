using GD1.Domain.Entities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using System.Reflection;

namespace GD1.Infrastructure.Data.Seeders
{
    public static class VehicleCatalogSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (!await context.VehicleCatalog.AnyAsync())
            {
                var filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "vehicles_master.json");
                
                // Fallback to source directory if running locally
                if (!File.Exists(filePath))
                {
                    filePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "GD1.Infrastructure", "Data", "vehicles_master.json");
                }
                
                if (File.Exists(filePath))
                {
                    var json = await File.ReadAllTextAsync(filePath);
                    var items = JsonSerializer.Deserialize<List<VehicleCatalogItem>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (items != null && items.Any())
                    {
                        await context.VehicleCatalog.AddRangeAsync(items);
                        await context.SaveChangesAsync();
                    }
                }
            }
        }
    }
}

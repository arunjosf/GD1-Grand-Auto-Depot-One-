using GD1.Application.Features.Vehicle.DTOs;
using GD1.Application.Interfaces;
using GD1.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GD1.Infrastructure.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly AppDbContext _context;

        public VehicleService(AppDbContext context)
        {
            _context = context;
        }

                        public async Task<List<VehicleLookupDto>> SearchAsync(string term, string? brand = null, string? category = null)
        {
            var query = _context.VehicleCatalog.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(brand))
            {
                query = query.Where(v => v.Brand == brand);
            }
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(v => v.Category == category);
            }
            if (!string.IsNullOrEmpty(term))
            {
                query = query.Where(v => v.Model.Contains(term) || v.Brand.Contains(term));
            }

            bool hasFilter = !string.IsNullOrEmpty(brand) || !string.IsNullOrEmpty(category) || !string.IsNullOrEmpty(term);

            // Fetch exactly 20 cars if no inputs are provided, otherwise return all filtered results
            var baseQuery = query
                .Select(v => new VehicleLookupDto 
                { 
                    Id = v.Id.ToString(),
                    Brand = v.Brand, 
                    Model = v.Model, 
                    Category = v.Category,
                    LogoUrl = GetLogo(v.Brand)
                });

            return hasFilter
                ? await baseQuery.ToListAsync()
                : await baseQuery.Take(20).ToListAsync();
        }

        public async Task<(bool IsValid, string Category)> ValidateVehicleYearAsync(string brand, string model, int year)
        {
            var car = await _context.VehicleCatalog
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Brand == brand && v.Model == model);

            if (car != null)
            {
                return (true, car.Category);
            }
            
            return (true, "Standard"); // Fallback
        }

        public async Task<(double Length, double Width, double Height)> GetDimensionsAsync(string brand, string model, string type)
        {
            var car = await _context.VehicleCatalog
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Brand == brand && v.Model == model);

            if (car != null)
            {
                return (car.LengthFeet, car.WidthFeet, car.HeightFeet);
            }

            return (16.0, 6.2, 5.0); // Fallback
        }

                        public async Task<VehicleLookupDto?> DecodeVinAsync(string vin)
        {
            if (string.IsNullOrWhiteSpace(vin) || vin.Length < 11) return null;

            try
            {
                using var client = new System.Net.Http.HttpClient();
                var response = await client.GetStringAsync($"https://vpic.nhtsa.dot.gov/api/vehicles/DecodeVinValues/{vin}?format=json");
                using var doc = System.Text.Json.JsonDocument.Parse(response);
                
                var root = doc.RootElement;
                if (!root.TryGetProperty("Results", out var results) || results.GetArrayLength() == 0)
                {
                    return null;
                }

                var result = results[0];
                var brand = result.GetProperty("Make").GetString()?.Trim().ToUpper();
                var model = result.GetProperty("Model").GetString()?.Trim();
                
                if (string.IsNullOrEmpty(brand) || string.IsNullOrEmpty(model)) return null;

                // Try to find the category in our catalog
                var dbCar = await _context.VehicleCatalog
                    .AsNoTracking()
                    .FirstOrDefaultAsync(v => v.Brand == brand && v.Model == model);

                var category = dbCar?.Category ?? "Standard";

                // Reject heavy vehicles per business rules
                if (category == "Bus" || category == "Heavy Truck" || category == "RV" || category == "Other")
                {
                    throw new InvalidOperationException("Heavy vehicles (Buses, RVs, Heavy Trucks) are not supported by this facility.");
                }

                return new VehicleLookupDto
                {
                    Id = dbCar?.Id.ToString() ?? "0",
                    Brand = brand,
                    Model = model,
                    Category = category,
                    LogoUrl = GetLogo(brand)
                };
            }
            catch (InvalidOperationException)
            {
                throw; // Rethrow business rule exceptions
            }
            catch
            {
                return null;
            }
        }

        private static string GetLogo(string brand) => $"https://logo.clearbit.com/{brand.ToLower().Replace(" ", "")}.com";
    }
}




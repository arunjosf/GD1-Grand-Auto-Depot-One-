using GD1.Application.Features.Vehicle.DTOs;
using GD1.Application.Interfaces;
using GD1.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;


    namespace GD1.Infrastructure.Services
    {
        public class VehicleService : IVehicleService
        {
            private readonly HttpClient _httpClient;
            private readonly List<string> _popularBrands = new() { "Toyota", "Suzuki", "Honda", "Hyundai", "Tata", "Mahindra", "Kia", "Ford", "BMW", "Mercedes" };

            public VehicleService(HttpClient httpClient) => _httpClient = httpClient;

            public async Task<List<VehicleLookupDto>> SearchAsync(string term, string? brand = null)
            {
                var results = new List<VehicleLookupDto>();

                // Case: Searching for Models within a selected Brand
                if (!string.IsNullOrEmpty(brand))
                {
                    var url = $"https://vpic.nhtsa.dot.gov/api/vehicles/GetModelsForMake/{brand}?format=json";
                    try
                    {
                        var response = await _httpClient.GetFromJsonAsync<NhtsaResponse>(url);
                        return response?.Results
                            .Where(r => string.IsNullOrEmpty(term) || r.Model_Name.Contains(term, StringComparison.OrdinalIgnoreCase))
                            .Select(r => new VehicleLookupDto { Brand = brand, Model = r.Model_Name, LogoUrl = GetLogo(brand) })
                            .Take(15).ToList() ?? new List<VehicleLookupDto>();
                    }
                    catch { return new List<VehicleLookupDto>(); }
                }

                // Case: Searching for Brands
                return _popularBrands
                    .Where(b => string.IsNullOrEmpty(term) || b.Contains(term, StringComparison.OrdinalIgnoreCase))
                    .Select(b => new VehicleLookupDto { Brand = b, LogoUrl = GetLogo(b) })
                    .ToList();
            }

            public async Task<(double Length, double Width, double Height)> GetDimensionsAsync(string brand, string model, string type)
            {
                // Heuristic values in feet (Length, Width, Height)
                // Limo: ~25ft, SUV: ~17ft, Sedan: ~15ft, Hatchback: ~13ft
                var t = type?.ToLower() ?? "unknown";
                var m = model?.ToLower() ?? "";

                if (m.Contains("limo") || m.Contains("limousine")) return (25.0, 6.5, 5.0);
                
                return t switch
                {
                    "suv" => (17.5, 6.5, 6.0),
                    "truck" => (19.0, 7.0, 6.5),
                    "van" => (18.0, 6.8, 7.0),
                    "sedan" => (15.5, 6.0, 4.8),
                    "luxury" => (17.0, 6.5, 4.8),
                    "sport" or "sports" => (14.5, 6.2, 4.0),
                    "hatchback" => (13.5, 5.8, 4.5),
                    "coupe" => (15.0, 6.2, 4.3),
                    "convertible" => (15.0, 6.0, 4.2),
                    "mpv" or "muv" => (16.5, 6.3, 5.8),
                    "compact" => (14.0, 5.9, 4.7),
                    "mini" => (11.5, 5.5, 4.8),
                    _ => (16.0, 6.2, 5.0) // Average fallback
                };
            }

            private string GetLogo(string brand) => $"https://logo.clearbit.com/{brand.ToLower().Replace(" ", "")}.com";
        }

        public class NhtsaResponse { public List<NhtsaResult> Results { get; set; } }
        public class NhtsaResult { public string Model_Name { get; set; } }
    }


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

            private string GetLogo(string brand) => $"https://logo.clearbit.com/{brand.ToLower().Replace(" ", "")}.com";
        }

        public class NhtsaResponse { public List<NhtsaResult> Results { get; set; } }
        public class NhtsaResult { public string Model_Name { get; set; } }
    }


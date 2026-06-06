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

                        private async Task<List<VehicleLookupDto>> FetchFromNhtsaAsync(string make, string filterTerm, string? category = null)
        {
            var types = new List<string>();
            
            // Map our system categories to NHTSA Vehicle Types
            if (string.IsNullOrEmpty(category) || category.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                types.AddRange(new[] { "passenger%20car", "multipurpose%20passenger%20vehicle", "truck" });
            }
            else if (category.Equals("Car", StringComparison.OrdinalIgnoreCase) || category.Equals("Sedan", StringComparison.OrdinalIgnoreCase))
            {
                types.Add("passenger%20car");
            }
            else if (category.Equals("SUV", StringComparison.OrdinalIgnoreCase))
            {
                types.Add("multipurpose%20passenger%20vehicle");
            }
            else if (category.Equals("Truck", StringComparison.OrdinalIgnoreCase))
            {
                types.Add("truck");
            }
            else
            {
                types.AddRange(new[] { "passenger%20car", "multipurpose%20passenger%20vehicle", "truck" });
            }

            var tasks = types.Select(async type => 
            {
                try 
                {
                    using var client = new System.Net.Http.HttpClient();
                    client.DefaultRequestHeaders.Add("User-Agent", "GrandAutoDepot/1.0");
                    var url = $"https://vpic.nhtsa.dot.gov/api/vehicles/GetModelsForMakeYear/make/{Uri.EscapeDataString(make)}/vehicletype/{type}?format=json";
                    var response = await client.GetStringAsync(url);
                    using var doc = System.Text.Json.JsonDocument.Parse(response);
                    var root = doc.RootElement;
                    if (root.TryGetProperty("Results", out var results))
                    {
                        var list = new List<VehicleLookupDto>();
                        foreach(var item in results.EnumerateArray())
                        {
                            var brand = item.GetProperty("Make_Name").GetString()?.Trim().ToUpper();
                            var model = item.GetProperty("Model_Name").GetString()?.Trim();
                            var nhtsaCat = item.GetProperty("VehicleTypeName").GetString()?.Trim() ?? "Standard";

                            // Map NHTSA categories to clean Body Types for UI
                            var cat = "Standard";
                            if (nhtsaCat.Equals("Passenger Car", StringComparison.OrdinalIgnoreCase)) cat = "Car";
                            else if (nhtsaCat.StartsWith("Multipurpose", StringComparison.OrdinalIgnoreCase)) cat = "SUV";
                            else if (nhtsaCat.Equals("Truck", StringComparison.OrdinalIgnoreCase)) cat = "Truck";

                            if (!string.IsNullOrEmpty(brand) && !string.IsNullOrEmpty(model))
                            {
                                if (string.IsNullOrEmpty(filterTerm) || 
                                    model.Contains(filterTerm, StringComparison.OrdinalIgnoreCase) || 
                                    brand.Contains(filterTerm, StringComparison.OrdinalIgnoreCase) ||
                                    (brand + " " + model).Contains(filterTerm, StringComparison.OrdinalIgnoreCase))
                                {
                                    list.Add(new VehicleLookupDto
                                    {
                                        Id = item.GetProperty("Model_ID").GetInt32().ToString(),
                                        Brand = brand,
                                        Model = model,
                                        Category = cat,
                                        ValidYearsCsv = "",
                                        LogoUrl = GetLogo(brand)
                                    });
                                }
                            }
                        }
                        return list;
                    }
                } 
                catch { }
                return new List<VehicleLookupDto>();
            });
            
            var resultsArray = await Task.WhenAll(tasks);
            return resultsArray.SelectMany(x => x)
                               .GroupBy(x => new { x.Brand, x.Model })
                               .Select(g => g.First())
                               .ToList();
        }

        public async Task<List<VehicleLookupDto>> SearchAsync(string term, string? brand = null, string? category = null)
        {
            if (string.IsNullOrWhiteSpace(term)) return new List<VehicleLookupDto>();
            
            // Extract explicit NHTSA categories typed within the search bar
            var lowerTerm = term.ToLowerInvariant();
            if (lowerTerm.Contains("mpv")) 
            { 
                category = "SUV"; 
                term = System.Text.RegularExpressions.Regex.Replace(term, "mpv", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Trim(); 
            }
            else if (lowerTerm.Contains("passenger car")) 
            { 
                category = "Car"; 
                term = System.Text.RegularExpressions.Regex.Replace(term, "passenger car", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Trim(); 
            }
            else if (lowerTerm.Contains("truck")) 
            { 
                category = "Truck"; 
                term = System.Text.RegularExpressions.Regex.Replace(term, "truck", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Trim(); 
            }
            else if (lowerTerm.Contains("van")) 
            { 
                // Map van to MPV (SUV internally)
                category = "SUV"; 
                term = System.Text.RegularExpressions.Regex.Replace(term, "van", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Trim(); 
            }

            var parts = term.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return new List<VehicleLookupDto>();
            
            var make1 = parts[0];
            var results = await FetchFromNhtsaAsync(make1, term, category);
            
            // Try 2-word make (e.g. Land Rover, Aston Martin) if 1-word failed
            if (results.Count == 0 && parts.Length > 1)
            {
                var make2 = parts[0] + " " + parts[1];
                results = await FetchFromNhtsaAsync(make2, term, category);
            }
            // Sort alphabetically by Model so that SUVs aren't pushed out by the Take(30) limit
            return results.OrderBy(x => x.Model).Take(30).ToList();
        }

        public async Task<(bool IsValid, string Category)> ValidateVehicleYearAsync(string brand, string model, int year)
        {
            if (year < 1900 || year > DateTime.UtcNow.Year + 1)
            {
                return (false, "Standard");
            }
            
            try 
            {
                using var client = new System.Net.Http.HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "GrandAutoDepot/1.0");
                var url = $"https://vpic.nhtsa.dot.gov/api/vehicles/GetModelsForMakeYear/make/{Uri.EscapeDataString(brand)}/modelyear/{year}?format=json";
                var response = await client.GetStringAsync(url);
                using var doc = System.Text.Json.JsonDocument.Parse(response);
                var root = doc.RootElement;
                if (root.TryGetProperty("Results", out var results))
                {
                    foreach(var item in results.EnumerateArray())
                    {
                        var nhtsaModel = item.GetProperty("Model_Name").GetString()?.Trim() ?? "";
                        bool isMatch = false;
                        
                        if (nhtsaModel.Equals(model, StringComparison.OrdinalIgnoreCase)) 
                        {
                            isMatch = true;
                        } 
                        else 
                        {
                            var modelWords = model.Split(new[] { ' ', '-' }, StringSplitOptions.RemoveEmptyEntries).Select(w => w.ToLower()).ToList();
                            var nhtsaWords = nhtsaModel.Split(new[] { ' ', '-' }, StringSplitOptions.RemoveEmptyEntries).Select(w => w.ToLower()).ToList();
                            var sharedWords = modelWords.Intersect(nhtsaWords).Where(w => w != "sedan" && w != "coupe" && w != "suv" && w != "truck").ToList();
                            if (sharedWords.Any()) 
                            { 
                                isMatch = true; 
                            }
                        }

                        if (isMatch)
                        {
                            string nhtsaCat = "Standard";
                            if (item.TryGetProperty("VehicleTypeName", out var typeProp))
                            {
                                nhtsaCat = typeProp.GetString()?.Trim() ?? "Standard";
                            }
                            
                            var cat = "Standard";
                            if (nhtsaCat.Equals("Passenger Car", StringComparison.OrdinalIgnoreCase)) cat = "Car";
                            else if (nhtsaCat.StartsWith("Multipurpose", StringComparison.OrdinalIgnoreCase)) cat = "SUV";
                            else if (nhtsaCat.Equals("Truck", StringComparison.OrdinalIgnoreCase)) cat = "Truck";
                            
                            return (true, cat);
                        }
                    }
                    return (false, "Standard");
                }
            } 
            catch (Exception ex)
            {
                // Log the exception internally if we had a logger, but throw to ensure frontend doesn't silently bypass
                throw new Exception("NHTSA API communication failed", ex);
            }
            
            return (false, "Standard");
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
                    ValidYearsCsv = dbCar?.ValidYearsCsv,
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




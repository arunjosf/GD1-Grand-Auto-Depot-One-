using GD1.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace GD1.Infrastructure.Services
{
    public class GeocodingService : IGeocodingService
    {
        private readonly HttpClient _httpClient;
        private readonly Microsoft.Extensions.Logging.ILogger<GeocodingService> _logger;

        public GeocodingService(HttpClient httpClient, Microsoft.Extensions.Logging.ILogger<GeocodingService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "GD1-Auto-Platform-v2");
        }

        public async Task<(double Lat, double Lon)?> GetCoordinatesAsync(string address)
        {
            var coords = await CallNominatimAsync(address);
            
            // If full address fails, try a simpler fallback (City, State)
            if (coords == null && address.Contains(","))
            {
                var parts = address.Split(',');
                if (parts.Length >= 2)
                {
                    var fallbackAddress = string.Join(",", parts.Skip(parts.Length - 2)); // Take last two parts (e.g. City, State)
                    _logger.LogInformation("Full address failed. Retrying with fallback: {Fallback}", fallbackAddress);
                    coords = await CallNominatimAsync(fallbackAddress);
                }
            }

            return coords;
        }

        private async Task<(double Lat, double Lon)?> CallNominatimAsync(string address)
        {
            try
            {
                var url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(address)}&format=json&limit=1";
                var response = await _httpClient.GetAsync(url);
                
                if (!response.IsSuccessStatusCode) return null;

                var data = await response.Content.ReadFromJsonAsync<List<NominatimResponse>>();
                if (data != null && data.Count > 0)
                {
                    var result = data[0];
                    if (double.TryParse(result.Lat, out double lat) && double.TryParse(result.Lon, out double lon))
                    {
                        return (lat, lon);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Geocoding error for {Address}", address);
            }
            return null;
        }

        private class NominatimResponse
        {
            [JsonPropertyName("lat")]
            public string Lat { get; set; } = string.Empty;

            [JsonPropertyName("lon")]
            public string Lon { get; set; } = string.Empty;

            [JsonPropertyName("display_name")]
            public string DisplayName { get; set; } = string.Empty;
        }
    }
}

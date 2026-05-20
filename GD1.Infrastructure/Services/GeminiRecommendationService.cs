using GD1.Application.Features.GD1Admin.DTOs;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace GD1.Infrastructure.Services
{
    // OBSOLETE: Use GeminiService instead
    public class GeminiRecommendationService
    {
        private readonly string _apiKey;
        private readonly HttpClient _http;

        public GeminiRecommendationService(IConfiguration config, HttpClient http)
        {
            _apiKey = config["Gemini:ApiKey"] ?? string.Empty;
            _http = http;
        }

        public async Task<string> GetRecommendationAsync(List<StoragePropertyListDto> lots, string userPreference)
        {
            return "GEMINI_JSON_RESPONSE";
        }
    }
}

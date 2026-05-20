using System.Net.Http.Json;
using System.Text.Json;
using GD1.Application.Features.GD1Admin.DTOs;
using GD1.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace GD1.Infrastructure.Services
{
    public class GeminiService : IGeminiService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;
        private readonly ILogger<GeminiService> _logger;

        public GeminiService(HttpClient http, IConfiguration config, ILogger<GeminiService> logger)
        {
            _http = http;
            _apiKey = config["Gemini:ApiKey"] ?? string.Empty;
            _logger = logger;
        }

        public async Task<AiRecommendationResponse> GetBestLotRecommendationAsync(List<StoragePropertyListDto> lots, string userPreference)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                return new AiRecommendationResponse { Reason = "Gemini API Key is not configured." };
            }

            if (lots == null || lots.Count == 0)
            {
                return new AiRecommendationResponse { Reason = "No private garages available for analysis." };
            }

            try
            {
                var lotData = string.Join("\n", lots.Select(l => 
                    $"- ID: {l.Id}, Name: {l.Name}, Rating: {l.AverageRating}, CCTV: {l.PropertyDetails.HasCCTV}, Security: {l.PropertyDetails.HasSecurity}, Workshop: {l.PropertyDetails.HasWorkshop}, Washing: {l.PropertyDetails.HasWashingArea}"));

                var prompt = $@"
                You are the GD1 Smart Private Garage Assistant. 
                Your task is to recommend the single best private garage property from the list below based on the user's preference.
                Note that these are premium private garages, not ordinary parking lots.

                User Preference: ""{userPreference}""

                Nearby Properties:
                {lotData}

                Instructions:
                1. Pick the best property ID (referenced as bestLotId).
                2. Provide a short, professional reason for the choice.
                3. Provide a brief analysis of why it's better than others (consider security, rating, and facilities).
                
                Respond ONLY with a JSON object in this exact format:
                {{
                  ""bestLotId"": 123,
                  ""reason"": ""Short summary"",
                  ""aiAnalysis"": ""Detailed but concise explanation""
                }}";

                var requestBody = new
                {
                    contents = new[]
                    {
                        new { parts = new[] { new { text = prompt } } }
                    }
                };

                var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={_apiKey}";
                
                var response = await _http.PostAsJsonAsync(url, requestBody);
                
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Gemini API Error: {Error}", error);
                    return new AiRecommendationResponse { Reason = "AI analysis failed at the moment." };
                }

                var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                var aiText = result.GetProperty("candidates")[0]
                                   .GetProperty("content")
                                   .GetProperty("parts")[0]
                                   .GetProperty("text")
                                   .GetString();

                if (aiText != null && aiText.Contains("```json"))
                {
                    aiText = aiText.Replace("```json", "").Replace("```", "").Trim();
                }

                var aiResult = JsonSerializer.Deserialize<AiRecommendationResponse>(aiText ?? "{}", new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return aiResult ?? new AiRecommendationResponse { Reason = "Could not parse AI response." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Gemini AI");
                return new AiRecommendationResponse { Reason = "An error occurred during AI analysis." };
            }
        }
    }
}

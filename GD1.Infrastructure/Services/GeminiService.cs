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

        public async Task<ImageReadabilityResponse> VerifyImageReadabilityAsync(string imageUrl, string expectedSubject)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                return new ImageReadabilityResponse { IsReadable = true, ConfidenceScore = 100, Reason = "Gemini API Key is not configured, skipping check." };
            }

            if (string.IsNullOrEmpty(imageUrl))
            {
                return new ImageReadabilityResponse { IsReadable = false, ConfidenceScore = 0, Reason = "No image URL provided." };
            }

            try
            {
                // Download image
                var imageResponse = await _http.GetAsync(imageUrl);
                if (!imageResponse.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to download image from {Url}", imageUrl);
                    return new ImageReadabilityResponse { IsReadable = true, ConfidenceScore = 100, Reason = "Could not download image, skipping check." };
                }

                var imageBytes = await imageResponse.Content.ReadAsByteArrayAsync();
                var base64Image = Convert.ToBase64String(imageBytes);
                var mimeType = imageResponse.Content.Headers.ContentType?.MediaType ?? "image/jpeg";

                var prompt = $@"
                You are an AI Image Quality Verifier for a car storage facility.
                Your task is to analyze this image to verify if it clearly shows the expected subject.
                
                Expected Subject: {expectedSubject}
                
                Instructions:
                1. Is the image clear and NOT blurry?
                2. Can you clearly identify the expected subject?
                3. Provide a confidence score (0-100).
                
                Respond ONLY with a JSON object in this exact format:
                {{
                  ""isReadable"": true,
                  ""confidenceScore"": 95,
                  ""reason"": ""The image clearly shows the vehicle's front exterior with good lighting and no blur.""
                }}";

                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new object[]
                            {
                                new { text = prompt },
                                new
                                {
                                    inlineData = new
                                    {
                                        mimeType = mimeType,
                                        data = base64Image
                                    }
                                }
                            }
                        }
                    }
                };

                var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={_apiKey}";
                
                var response = await _http.PostAsJsonAsync(url, requestBody);
                
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Gemini Vision API Error: {Error}", error);
                    return new ImageReadabilityResponse { IsReadable = true, ConfidenceScore = 100, Reason = "AI analysis failed, allowing upload." };
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

                var aiResult = JsonSerializer.Deserialize<ImageReadabilityResponse>(aiText ?? "{}", new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return aiResult ?? new ImageReadabilityResponse { IsReadable = true, ConfidenceScore = 100, Reason = "Could not parse AI response." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Gemini Vision AI");
                return new ImageReadabilityResponse { IsReadable = true, ConfidenceScore = 100, Reason = "An error occurred, allowing upload." };
            }
        }
    }
}

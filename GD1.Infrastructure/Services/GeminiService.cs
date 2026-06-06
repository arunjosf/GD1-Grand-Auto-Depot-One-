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
            _apiKey = (config["Gemini:ApiKey"] ?? string.Empty).Trim();
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
                    $"- ID: {l.Id}, Name: {l.Name}, Rating: {l.AverageRating}, CCTV: {l.PropertyDetails.HasCCTV}, Security: {l.PropertyDetails.HasSecurity}, Workshop: {l.PropertyDetails.HasWorkshop}, Washing: {l.PropertyDetails.HasWashingArea}, Reviews: [{(l.RecentReviews.Any() ? string.Join(" | ", l.RecentReviews) : "No reviews yet")}]"));

                var prompt = $@"
                You are the GD1 Smart Private Garage Assistant. 
                Your task is to recommend the single best private garage property from the list below based on the user's preference, available amenities, and most importantly, the user-typed reviews.
                Note that these are premium private garages, not ordinary parking lots. Read the user reviews carefully to determine actual quality.

                User Preference: ""{userPreference}""

                Nearby Properties:
                {lotData}

                Instructions:
                1. Pick the best property ID (referenced as bestLotId).
                2. Provide a short, professional reason for the choice.
                3. Provide a brief analysis of why it's better than others (consider security, rating, facilities, and specifically mention positive elements from the user reviews if any).
                
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

                var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_apiKey}";
                
                var response = await _http.PostAsJsonAsync(url, requestBody);
                
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Gemini API Error: {Error}", error);
                    try { System.IO.File.WriteAllText(@"C:\Users\HP\.gemini\antigravity\gemini_error.txt", error); } catch {}
                    return new AiRecommendationResponse { Reason = "AI analysis failed at the moment." };
                }

                var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                var aiText = result.GetProperty("candidates")[0]
                                   .GetProperty("content")
                                   .GetProperty("parts")[0]
                                   .GetProperty("text")
                                   .GetString();

                if (aiText != null && aiText.Contains("```"))
                {
                    aiText = aiText.Replace("```json", "").Replace("```", "").Trim();
                }

                var options = new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true,
                    NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
                };
                try { System.IO.File.WriteAllText(@"C:\Users\HP\.gemini\antigravity\gemini_debug.txt", aiText ?? "null"); } catch {}
                var aiResult = JsonSerializer.Deserialize<AiRecommendationResponse>(aiText ?? "{}", options);
                return aiResult ?? new AiRecommendationResponse { Reason = "Could not parse AI response." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Gemini AI");
                return new AiRecommendationResponse { Reason = "An error occurred during AI analysis." };
            }
        }

        public async Task<AiServiceCenterRecommendationResponse> GetBestServiceCenterRecommendationAsync(string serializedServiceCenters)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                return new AiServiceCenterRecommendationResponse { Reason = "Gemini API Key is not configured." };
            }

            if (string.IsNullOrEmpty(serializedServiceCenters))
            {
                return new AiServiceCenterRecommendationResponse { Reason = "No service centers available for analysis." };
            }

            try
            {
                var prompt = $@"
                You are the GD1 Smart Auto Service Assistant. 
                Your task is to recommend the single best service center from the list below based on their reviews, ratings, and proximity.

                Nearby Service Centers:
                {serializedServiceCenters}

                Instructions:
                1. Pick the best service center ID (referenced as bestServiceCenterId).
                2. Provide a short, professional reason for the choice.
                3. Provide a brief analysis of why it's better than others.
                
                Respond ONLY with a JSON object in this exact format:
                {{
                  ""bestServiceCenterId"": 123,
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

                var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key={_apiKey}";
                
                var response = await _http.PostAsJsonAsync(url, requestBody);
                
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Gemini API Error: {Error}", error);
                    return new AiServiceCenterRecommendationResponse { Reason = "AI analysis failed at the moment." };
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

                var aiResult = JsonSerializer.Deserialize<AiServiceCenterRecommendationResponse>(aiText ?? "{}", new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return aiResult ?? new AiServiceCenterRecommendationResponse { Reason = "Could not parse AI response." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Gemini AI");
                return new AiServiceCenterRecommendationResponse { Reason = "An error occurred during AI analysis." };
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

                var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key={_apiKey}";
                
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

        public async Task<string> GetFaqChatResponseAsync(string userMessage)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                return "AI assistant is currently unavailable.";
            }

            try
            {
                var prompt = $@"
                You are a helpful and polite AI Assistant for 'Grand Auto Depot One' (GD1). 
                GD1 is a platform that connects Vehicle Owners with Private Garages for long-term storage, 
                and provides on-demand vehicle pickup, delivery, and maintenance services through our Lot Managers and Service Centers.

                Here is the core knowledge of the GD1 Platform you should use to answer questions:
                1. Roles: 
                   - Vehicle Owners: Can book private garages, request vehicle pickup, track rides, and request maintenance.
                   - Lot Owners: Can list their empty private garages for storage, manage bookings, and hire Lot Managers.
                   - Lot Managers: Act as agents for the Lot Owners. They handle vehicle pickup, drop-off, OTP verification, and image condition reporting.
                   - Service Centers: Third-party workshops that partner with GD1 to provide maintenance and repair services.
                2. Franchise Applications: Users can apply to become a Lot Owner or a Service Center partner by submitting a Franchise Application. An admin will review and approve/reject it.
                3. Booking Process: A vehicle owner selects a garage, books it, signs a digital agreement, and can optionally request a pickup manager.
                4. Real-time Features: The platform has real-time tracking, live notifications, OTP security for vehicle handovers, and AI image readability verification for damage protection.
                5. Pricing: Pricing is set per-day by the Lot Owner. Payments and digital agreements must be signed before the vehicle is stored.

                The user is asking a question on the platform. Answer clearly, politely, and concisely using the knowledge above.
                Do not provide any code or unrelated information. If asked something outside GD1's scope, politely redirect them to GD1 services.
                
                User Message: {userMessage}";

                var requestBody = new
                {
                    contents = new[]
                    {
                        new { parts = new[] { new { text = prompt } } }
                    }
                };

                var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key={_apiKey}";
                
                var response = await _http.PostAsJsonAsync(url, requestBody);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Gemini API Error for Chatbot: {Status}", response.StatusCode);
                    return "Sorry, I am having trouble connecting to the network right now.";
                }

                var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                var aiText = result.GetProperty("candidates")[0]
                                   .GetProperty("content")
                                   .GetProperty("parts")[0]
                                   .GetProperty("text")
                                   .GetString();

                return aiText?.Trim() ?? "I'm sorry, I couldn't formulate a response.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Gemini Chat AI");
                return "Sorry, an unexpected error occurred while processing your request.";
            }
        }
    }
}

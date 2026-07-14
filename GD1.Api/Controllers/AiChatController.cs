using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GD1.Api.Controllers
{
    [Route("api/aichat")]
    [ApiController]
    public class AiChatController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public AiChatController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> AskQuestion([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Message))
                return BadRequest(new { reply = "Message cannot be empty." });

            var apiKey = _configuration["AI:GoogleApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
                return StatusCode(500, new { reply = "AI service is not configured." });

            // Google Gemini REST API endpoint (Google AI Studio - Free Tier)
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash-lite:generateContent?key={apiKey}";

            // Build the request body exactly as Google expects it
            var requestBody = new
            {
                system_instruction = new
                {
                    parts = new[]
                    {
                        new { text = "You are the official customer support assistant for GD1 (Grand Auto Depot One), a comprehensive vehicle storage, maintenance, and valet management platform. You are polite, professional, and helpful. Keep your answers concise and relevant. If someone asks something completely unrelated to vehicles, parking, or vehicle storage, politely let them know you can only help with GD1-related topics." }
                    }
                },
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[] { new { text = request.Message } }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.7,
                    maxOutputTokens = 512
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            var client = _httpClientFactory.CreateClient();

            var response = await client.PostAsync(url, httpContent);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, new { reply = $"AI service error: {response.StatusCode}", detail = errorBody });
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseBody);

            // Extract the text from Google's response structure
            var replyText = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return Ok(new { reply = replyText });
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; }
    }
}
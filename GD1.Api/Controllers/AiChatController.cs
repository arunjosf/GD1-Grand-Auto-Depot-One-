using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
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

            var apiKey = _configuration["AI:GroqApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
                return StatusCode(500, new { reply = "AI service is not configured." });

            // Groq API endpoint - OpenAI compatible format, 100% free tier
            var url = "https://api.groq.com/openai/v1/chat/completions";

            // Build the request body in OpenAI-compatible format
            var requestBody = new
            {
                model = "llama3-8b-8192", // Free Llama 3 model on Groq
                messages = new[]
                {
                    new
                    {
                        role = "system",
                        content = "You are the official customer support assistant for GD1 (Grand Auto Depot One), a comprehensive vehicle storage, maintenance, and valet management platform. You are polite, professional, and helpful. Keep your answers concise and relevant. If someone asks something completely unrelated to vehicles, parking, or vehicle storage, politely let them know you can only help with GD1-related topics."
                    },
                    new
                    {
                        role = "user",
                        content = request.Message
                    }
                },
                temperature = 0.7,
                max_tokens = 512
            };

            var json = JsonSerializer.Serialize(requestBody);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            var client = _httpClientFactory.CreateClient();

            // Groq uses Bearer token authentication
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var response = await client.PostAsync(url, httpContent);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, new { reply = $"AI service error: {response.StatusCode}", detail = errorBody });
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseBody);

            // Extract text from OpenAI-compatible response format
            var replyText = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return Ok(new { reply = replyText });
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; }
    }
}
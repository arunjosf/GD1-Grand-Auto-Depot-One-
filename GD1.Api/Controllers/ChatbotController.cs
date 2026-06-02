using GD1.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GD1.Api.Controllers
{
    [Route("api/chatbot")]
    [ApiController]
    public class ChatbotController : ControllerBase
    {
        private readonly IGeminiService _geminiService;

        public ChatbotController(IGeminiService geminiService)
        {
            _geminiService = geminiService;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] ChatbotRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new { Message = "Message cannot be empty." });
            }

            var response = await _geminiService.GetFaqChatResponseAsync(request.Message);
            return Ok(new { Answer = response });
        }
    }

    public class ChatbotRequest
    {
        public string Message { get; set; } = string.Empty;
    }
}

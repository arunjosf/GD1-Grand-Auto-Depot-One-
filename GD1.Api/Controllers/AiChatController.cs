using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Threading.Tasks;
using System.Text.Json;

namespace GD1.Api.Controllers
{
    [Route("api/aichat")]
    [ApiController]
    public class AiChatController : ControllerBase
    {
        private readonly Kernel _kernel;

        public AiChatController(Kernel kernel)
        {
            _kernel = kernel;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> AskQuestion([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest("Message cannot be empty.");

            var chatService = _kernel.GetRequiredService<IChatCompletionService>();

            var chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage(
                "You are the official customer support assistant for GD1 (Grand Auto Depot One). " +
                "You are polite, professional, and helpful. " +
                "Always keep your answers concise. If the user asks something completely unrelated to vehicles or parking, politely decline to answer."
            );

            chatHistory.AddUserMessage(request.Message);

            var response = await chatService.GetChatMessageContentAsync(chatHistory, kernel: _kernel);

            return Ok(new { reply = response.Content });
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MediatR;
using System.Threading.Tasks;

namespace GD1.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ChatController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("userId")?.Value
                ?? User.FindFirst("sub")?.Value;

            if (!long.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var result = await _mediator.Send(new GD1.Application.Features.Chat.Queries.GetConversationsQuery
            {
                UserId = userId
            });

            return Ok(result);
        }

        [HttpGet("history/{category}/{referenceId}")]
        public async Task<IActionResult> GetChatHistory(string category, long referenceId)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("userId")?.Value
                ?? User.FindFirst("sub")?.Value;

            if (!long.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var query = new GD1.Application.Features.Chat.Queries.GetChatHistoryQuery
            {
                UserId = userId
            };

            if (category == "garage")
                query.BookingId = referenceId;
            else if (category == "serviceCenter")
                query.ServiceRequestId = referenceId;
            else if (category == "manager")
                query.DirectUserId = referenceId;
            else
                return BadRequest("Invalid category");

            var result = await _mediator.Send(query);

            return Ok(result);
        }
    }
}

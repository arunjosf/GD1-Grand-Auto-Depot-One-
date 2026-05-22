using GD1.Application.Features.GD1Admin.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GD1.Api.Controllers
{
    [Route("api/access")]
    [ApiController]
    [Authorize(Roles = "GD1Admin,LotOwner")]
    public class AccessController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AccessController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("pending-requests")]
        public async Task<IActionResult> GetPendingRequests()
        {
            long? lotOwnerId = IsLotOwner() ? GetUserId() : null;
            var result = await _mediator.Send(new GetPendingAgentsQuery { LotOwnerId = lotOwnerId });
            return Ok(result);
        }

        [HttpPost("review-access")]
        public async Task<IActionResult> ReviewStaffAccess([FromForm] ReviewAccessRequest request)
        {
            var command = new ReviewAgentRequestCommand
            {
                Id = request.Id,
                Status = request.Status,
                LotOwnerId = IsLotOwner() ? GetUserId() : null
            };
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("users/{id}/toggle-block-or-unblock")]
        public async Task<IActionResult> ToggleUserStatus(long id)
        {
            long? lotOwnerId = IsLotOwner() ? GetUserId() : null;
            var result = await _mediator.Send(new ToggleUserStatusCommand { UserId = id, LotOwnerId = lotOwnerId });
            return Ok(result);
        }

        private long GetUserId()
        {
            var value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("userId")?.Value
                ?? throw new System.UnauthorizedAccessException("User not found in token.");
            return long.Parse(value);
        }

        private bool IsLotOwner() => User.IsInRole("LotOwner");
    }

    public class ReviewAccessRequest
    {
        public long Id { get; set; }
        public GD1.Domain.Entities.Enums.AgentApprovalStatus Status { get; set; }
    }
}

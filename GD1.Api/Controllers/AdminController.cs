using GD1.Application.Features.Agents.Commands;
using GD1.Application.Features.GD1Admin.Commands;
using GD1.Application.Features.GD1Admin.Queries;
using GD1.Application.Features.FranchiseApplication.DTOs;
using GD1.Application.Features.FranchiseApplication.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GD1.Api.Controllers
{
    [Route("api/admin")]
    [ApiController]
    [Authorize(Roles = "GD1Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminController(IMediator mediator) => _mediator = mediator;

        [HttpGet("agents")]
        public async Task<IActionResult> GetAllAgents()
        {
            var result = await _mediator.Send(new GetAllAgentsQuery());
            return Ok(result);
        }

        [HttpPost("agents/register-onboard")]
        public async Task<IActionResult> OnboardAgent([FromBody] InviteOrUpgradeAgentCommand cmd)
        {
            var result = await _mediator.Send(cmd);
            return Ok(result);
        }

        [HttpGet("agents/pending-login-requests")]
        public async Task<IActionResult> GetPendingRequests()
        {
            var result = await _mediator.Send(new GetPendingAgentsQuery());
            return Ok(result);
        }

        [HttpPost("agents/login-access")]
        public async Task<IActionResult> ReviewAgentAccess([FromBody] ReviewAgentRequestCommand cmd)
        {
            var result = await _mediator.Send(cmd);
            return Ok(result);
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers([FromQuery] GD1.Domain.Entities.Enums.UserRole? role, [FromQuery] string? search)
        {
            var result = await _mediator.Send(new GetAllUsersQuery { Role = role, SearchTerm = search });
            return Ok(result);
        }

        [HttpPost("users/{id}/toggle-block-or-unblock")]
        public async Task<IActionResult> ToggleUserStatus(long id)
        {
            var result = await _mediator.Send(new ToggleUserStatusCommand { UserId = id });
            return Ok(result);
        }


        [HttpGet("franchise/applications")]
        public async Task<IActionResult> GetAllApplications([FromQuery] GD1.Domain.Entities.Enums.FranchiseStatus? status, [FromQuery] string? search, [FromQuery] string? sortBy = "CreatedAt", [FromQuery] bool descending = true)
        {
            var result = await _mediator.Send(new GetAllApplicationsQuery 
            { 
                Status = status,
                SearchTerm = search, 
                SortBy = sortBy, 
                Descending = descending 
            });
            return Ok(result);
        }

        [HttpGet("franchise/applications/{id}")]
        public async Task<IActionResult> GetApplicationDetail(long id)
        {
            var result = await _mediator.Send(new GetApplicationDetailQuery { Id = id });
            return Ok(result);
        }

        [HttpPatch("franchise/applications/{id}/status")]
        public async Task<IActionResult> UpdateStatus(long id, [FromBody] UpdateStatusRequest req)
        {
            var result = await _mediator.Send(new UpdateApplicationStatusCommand
            {
                Id = id,
                Status = req.Status,
                AdminNotes = req.AdminNotes,
                AdminId = GetUserId()
            });
            return Ok(result);
        }

        [HttpGet("franchise/applications/{id}/nearby-agents")]
        public async Task<IActionResult> GetNearbyAgents(long id)
        {
            var result = await _mediator.Send(new GetNearbyAgentsQuery { ApplicationId = id });
            return Ok(result);
        }

        [HttpPost("franchise/applications/{id}/assign-agent")]
        public async Task<IActionResult> AssignAgent(long id, [FromBody] AssignAgentRequest req)
        {
            var result = await _mediator.Send(new AssignAgentCommand
            {
                ApplicationId = id,
                AgentId = req.AgentId,
                ScheduledDate = req.ScheduledDate,
                AdminId = GetUserId()
            });
            return Ok(result);
        }

        [HttpPost("franchise/agent/inspection-reports/{id}/review")]
        public async Task<IActionResult> ReviewInspection(long id, [FromBody] ReviewInspectionRequest req)
        {
            var result = await _mediator.Send(new ReviewInspectionCommand
            {
                ReportId = id,
                AdminId = GetUserId(),
                Decision = req.Decision,
                AdminRemarks = req.AdminRemarks
            });
            return Ok(result);
        }

        [HttpPost("franchise/agent/assignments/{id}/cancel")]
        public async Task<IActionResult> CancelAssignment(long id, [FromBody] CancelAssignmentRequest req)
        {
            var result = await _mediator.Send(new CancelAssignmentCommand
            {
                AssignmentId = id,
                Reason = req.Reason,
                AdminId = GetUserId()
            });
            return Ok(result);
        }

        [HttpPost("franchise/agent/appeals/{id}/review")]
        public async Task<IActionResult> ReviewAgentAppeal(long id, [FromBody] ReviewAppealRequest req)
        {
            var result = await _mediator.Send(new ReviewAppealCommand
            {
                RequestId = id,
                Decision = req.Decision,
                Reason = req.Reason,
                AdminId = GetUserId()
            });
            return Ok(result);
        }

        [HttpGet("partnered-lots")]
        public async Task<IActionResult> GetPartneredStorageLots()
        {
            var result = await _mediator.Send(new GetAllStoragePropertyQuery());
            return Ok(result);
        }

        private long GetUserId()
        {
            var value = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("userId")?.Value
                ?? throw new UnauthorizedAccessException("User not found in token.");
            return long.Parse(value);
        }
    }

    public class AssignAgentRequest
    {
        public long AgentId { get; set; }
        public DateTime ScheduledDate { get; set; }
    }

    public class ReviewInspectionRequest
    {
        public GD1.Domain.Entities.Enums.InspectionDecision Decision { get; set; }
        public string? AdminRemarks { get; set; }
    }

    public class UpdateStatusRequest
    {
        public GD1.Domain.Entities.Enums.FranchiseStatus Status { get; set; }
        public string? AdminNotes { get; set; }
    }

    public class ReviewAppealRequest
    {
        public GD1.Domain.Entities.Enums.AppealDecision Decision { get; set; }
        public string? Reason { get; set; }
    }

    public class CancelAssignmentRequest
    {
        public string? Reason { get; set; }
    }
}

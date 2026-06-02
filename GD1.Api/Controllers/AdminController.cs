using GD1.Application.Features.Agents.Commands;
using GD1.Application.Features.GD1Admin.Commands;
using GD1.Application.Features.GD1Admin.Queries;
using GD1.Application.Features.GD1Admin.DTOs;
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


        [HttpPost("agents/register-onboard")]
        public async Task<IActionResult> OnboardAgent([FromBody] InviteOrUpgradeAgentCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }


        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers([FromQuery] UserFilterRole? role, [FromQuery] string? search)
        {
            var result = await _mediator.Send(new GetAllUsersQuery { Role = role, SearchTerm = search });
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

        [HttpPost("franchise/applications/{id}/update-status")]
        public async Task<IActionResult> UpdateStatus(long id, [FromForm] UpdateStatusRequest req)
        {
            var result = await _mediator.Send(new UpdateApplicationStatusCommand
            {
                Id = id,
                Decision = req.Decision,
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

        [HttpGet("service-centers/applications")]
        public async Task<IActionResult> GetAllServiceCenters([FromQuery] long? id, [FromQuery] string? status, [FromQuery] string? search, [FromQuery] string? sortBy = "CreatedAt", [FromQuery] bool descending = true)
        {
            var result = await _mediator.Send(new GetAllServiceCenterApplicationsQuery
            {
                Id = id,
                Status = status,
                SearchTerm = search,
                SortBy = sortBy,
                Descending = descending
            });
            return Ok(result);
        }

        [HttpPost("service-centers/{id}/update-status")]
        public async Task<IActionResult> UpdateServiceCenterStatus(long id, [FromForm] UpdateSCStatusRequest req)
        {
            var result = await _mediator.Send(new UpdateServiceCenterStatusCommand
            {
                Id = id,
                Decision = req.Decision,
                AdminNotes = req.AdminNotes,
                AdminId = GetUserId()
            });
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



        private long GetUserId()
        {
            var value = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("userId")?.Value
                ?? throw new UnauthorizedAccessException("User not found in token.");
            return long.Parse(value);
        }

        private bool IsLotOwner() =>
            User.IsInRole("LotOwner");
    }

    public class AssignAgentRequest
    {
        public long AgentId { get; set; }
        public DateTime ScheduledDate { get; set; }
    }


    public class UpdateStatusRequest
    {
        public GD1.Domain.Entities.Enums.ApplicationReviewDecision Decision { get; set; }
        public string? AdminNotes { get; set; }
    }

    public class CancelAssignmentRequest
    {
        public string? Reason { get; set; }
    }

    public class UpdateSCStatusRequest
    {
        public GD1.Domain.Entities.Enums.ApplicationReviewDecision Decision { get; set; }
        public string? AdminNotes { get; set; }
    }
}

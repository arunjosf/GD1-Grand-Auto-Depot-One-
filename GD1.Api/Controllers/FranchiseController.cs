using GD1.Application.Features.FranchiseApplication.Commands;
using GD1.Application.Features.FranchiseApplication.DTOs;
using GD1.Application.Features.GD1Admin.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GD1.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FranchiseController : ControllerBase
    {
        private readonly IMediator _mediator;

        public FranchiseController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("apply")]
        [Authorize]
        public async Task<IActionResult> Apply(
            [FromBody] SubmitApplicationRequest req)
        {
            var result = await _mediator.Send(
                new SubmitApplicationCommand
                {
                    Request = req,
                    ApplicantId = GetUserId()
                });
            return Ok(result);
        }

        [HttpPost("{id}/assign-agent")]
        [Authorize(Roles = "GD1Admin")]
        public async Task<IActionResult> AssignAgent(
            long id, [FromBody] AssignAgentRequest req)
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

        [HttpGet("applications")]
        public async Task<IActionResult> GetAllApplications([FromQuery] string? search, [FromQuery] string? sortBy = "CreatedAt", [FromQuery] bool descending = true)
        {
            var result = await _mediator.Send(new GetAllApplicationsQuery { SearchTerm = search, SortBy = sortBy, Descending = descending });
            return Ok(result);
        }

        [HttpGet("applications/{id}/nearby-agents")]
        public async Task<IActionResult> GetNearbyAgents(long id)
        {
            var result = await _mediator.Send(new GetNearbyAgentsQuery { ApplicationId = id });
            return Ok(result);
        }

        [HttpPost("inspect/{token}")]
        [AllowAnonymous]
        public async Task<IActionResult> SubmitInspection(
            string token, [FromBody] SubmitInspectionRequest req)
        {
            var result = await _mediator.Send(
                new SubmitInspectionCommand
                {
                    AccessToken = token,
                    Request = req
                });
            return Ok(result);
        }

        [HttpPost("reports/{id}/review")]
        [Authorize(Roles = "GD1Admin")]
        public async Task<IActionResult> ReviewInspection(
            long id, [FromBody] ReviewInspectionRequest req)
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

        private long GetUserId()
        {
            var value = User.FindFirst("userId")?.Value
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
        public string Decision { get; set; } = string.Empty;
        public string? AdminRemarks { get; set; }
    }
}

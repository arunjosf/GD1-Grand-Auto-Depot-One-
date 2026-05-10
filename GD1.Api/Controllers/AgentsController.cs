using GD1.Application.Features.Agents.Commands;
using GD1.Application.Features.Agents.Queries;
using GD1.Application.Features.FranchiseApplication.DTOs;
using GD1.Domain.Entities;
using GD1.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GD1.Api.Controllers
{
    [Route("api/agents")]
    [ApiController]
    public class AgentsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AgentsController(IMediator mediator) => _mediator = mediator;

        [HttpPost("onboarding/finalize")]
        [AllowAnonymous]
        public async Task<IActionResult> FinalizeOnboarding([FromBody] FinalizeAgentOnboardingCommand cmd)
        {
            var result = await _mediator.Send(cmd);
            
            if (result.Success && result.Data != null)
            {
                SetTokenCookies(result.Data.AccessToken, result.Data.RefreshToken);
            }
            
            return Ok(result);
        }

        [HttpGet("my-inspections")]
        [Authorize(Roles = "Agent")]
        public async Task<IActionResult> GetMyInspections([FromServices] IGenericRepository<Agent> agentRepo)
        {
            var userId = GetUserId();
            var agent = (await agentRepo.FindAsync(a => a.UserId == userId)).FirstOrDefault();

            if (agent == null || !agent.IsVerified)
            {
                return StatusCode(403, new { success = false, message = "Access Denied. Your agent account is not yet verified." });
            }

            var result = await _mediator.Send(new GetMyAssignedInspectionsQuery 
            { 
                AgentId = agent.Id 
            });
            return Ok(result);
        }

        [HttpPost("assignments/{id}/submit-inspection")]
        [Authorize(Roles = "Agent")]
        public async Task<IActionResult> SubmitInspection(long id, [FromBody] PropertyInspectionSubmission req)
        {
            var result = await _mediator.Send(
                new SubmitInspectionCommand
                {
                    AssignmentId = id,
                    Request = req,
                    UserId = GetUserId()
                });
            return Ok(result);
        }

        [HttpPost("assignments/{id}/appeal")]
        [Authorize(Roles = "Agent")]
        public async Task<IActionResult> SubmitAppeal(long id, [FromBody] SubmitAppealRequest req)
        {
            var result = await _mediator.Send(new SubmitAppealCommand
            {
                AssignmentId = id,
                Description = req.Description,
                RescheduleRequestDate = req.RescheduleRequestDate,
                UserId = GetUserId()
            });
            return Ok(result);
        }

        private void SetTokenCookies(string accessToken, string refreshToken)
        {
            var opts = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("AccessToken", accessToken, opts);
            Response.Cookies.Append("RefreshToken", refreshToken, opts);
        }

        private long GetUserId()
        {
            var value = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("userId")?.Value
                ?? throw new UnauthorizedAccessException("User not found in token.");
            return long.Parse(value);
        }
    }

    public class SubmitAppealRequest
    {
        public string Description { get; set; } = string.Empty;
        public DateTime? RescheduleRequestDate { get; set; }
    }
}

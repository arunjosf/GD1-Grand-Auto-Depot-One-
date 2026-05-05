using GD1.Application.Features.FranchiseApplication.Commands;
using GD1.Application.Features.FranchiseApplication.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GD1.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FranchiseController : ControllerBase
    {
        private readonly SubmitApplicationCommandHandler _submit;
        private readonly AssignAgentCommandHandler _assign;
        private readonly SubmitInspectionCommandHandler _inspect;
        private readonly ReviewInspectionCommandHandler _review;

        public FranchiseController(
            SubmitApplicationCommandHandler submit,
            AssignAgentCommandHandler assign,
            SubmitInspectionCommandHandler inspect,
            ReviewInspectionCommandHandler review)
        {
            _submit = submit;
            _assign = assign;
            _inspect = inspect;
            _review = review;
        }

        [HttpPost("apply")]
        [Authorize]
        public async Task<IActionResult> Apply(
            [FromBody] SubmitApplicationRequest req)
        {
            var result = await _submit.HandleAsync(
                new SubmitApplicationCommand
                {
                    Request = req,
                    ApplicantId = GetUserId()
                });
            return Ok(result);
        }

        [HttpPost("{id}/assign-agent")]
        [Authorize]
        public async Task<IActionResult> AssignAgent(
            long id, [FromBody] AssignAgentRequest req)
        {
            var result = await _assign.HandleAsync(new AssignAgentCommand
            {
                ApplicationId = id,
                AgentId = req.AgentId,
                ScheduledDate = req.ScheduledDate,
                AdminId = GetUserId()
            });
            return Ok(result);
        }

        [HttpPost("inspect/{token}")]
        [AllowAnonymous]
        public async Task<IActionResult> SubmitInspection(
            string token, [FromBody] SubmitInspectionRequest req)
        {
            var result = await _inspect.HandleAsync(
                new SubmitInspectionCommand
                {
                    AccessToken = token,
                    Request = req
                });
            return Ok(result);
        }

        [HttpPost("reports/{id}/review")]
        [Authorize]
        public async Task<IActionResult> ReviewInspection(
            long id, [FromBody] ReviewInspectionRequest req)
        {
            var result = await _review.HandleAsync(new ReviewInspectionCommand
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


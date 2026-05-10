using GD1.Application.Features.FranchiseApplication.Commands;
using GD1.Application.Features.FranchiseApplication.DTOs;
using GD1.Application.Features.FranchiseApplication.Queries;
using GD1.Application.Features.GD1Admin.Queries;
using GD1.Application.Features.GD1Admin.Commands;
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

        [HttpGet("my-applications")]
        [Authorize]
        public async Task<IActionResult> GetMyApplications()
        {
            var result = await _mediator.Send(new GetMyApplicationsQuery 
            { 
                ApplicantId = GetUserId() 
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
    }

}

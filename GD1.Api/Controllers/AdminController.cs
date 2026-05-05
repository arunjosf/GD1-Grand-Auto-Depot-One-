using GD1.Application.Features.GD1Admin.Commands;
using GD1.Application.Features.GD1Admin.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GD1.Api.Controllers
{
    [Route("api/admin")]
    [ApiController]
    [Authorize(Roles = "GD1Admin")] // Ensure only Admin can access
    public class AdminController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("agents")]
        public async Task<IActionResult> GetAllAgents()
        {
            var result = await _mediator.Send(new GetAllAgentsQuery());
            return Ok(result);
        }

        [HttpPost("agents")]
        public async Task<IActionResult> AddAgent([FromBody] AddAgentCommand cmd)
        {
            var result = await _mediator.Send(cmd);
            return Ok(result);
        }

        [HttpPost("agents/{id}/block")]
        public async Task<IActionResult> BlockAgent(long id)
        {
            var result = await _mediator.Send(new BlockAgentCommand { AgentId = id });
            return Ok(result);
        }

        [HttpPost("users/{id}/block")]
        public async Task<IActionResult> BlockUser(long id)
        {
            var result = await _mediator.Send(new BlockUserCommand { UserId = id });
            return Ok(result);
        }

        [HttpGet("partnered-lots")]
        public async Task<IActionResult> GetPartneredStorageLots()
        {
            var result = await _mediator.Send(new GetAllStoragePropertyQuery());
            return Ok(result);
        }
    }
}

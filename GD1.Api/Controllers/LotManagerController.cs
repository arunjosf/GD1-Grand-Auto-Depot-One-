using GD1.Application.Features.LotManagement.Commands;
using GD1.Application.Features.LotManagement.Queries;
using GD1.Application.Features.Pickup.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GD1.Api.Controllers
{
    [Route("api/lot-manager")]
    [ApiController]
    [Authorize(Roles = "LotOwner,Manager")]
    public class LotManagerController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LotManagerController(IMediator mediator) => _mediator = mediator;

 
        [HttpPost("properties/{propertyId}/invite-manager")]
        [Authorize(Roles = "LotOwner")]
        public async Task<IActionResult> InviteManager(long propertyId, [FromBody] InviteManagerRequest req)
        {
            var result = await _mediator.Send(new AddLotManagerCommand
            {
                LotOwnerId = GetUserId(),
                PropertyId = propertyId,
                ManagerEmail = req.ManagerEmail,
                ManagerFullName = req.ManagerFullName,
                IdProofUrl = req.IdProofUrl,
                SelfieUrl = req.SelfieUrl
            });
            return Ok(result);
        }

        [HttpGet("manager/my-assignments")]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Manager")]
        public async Task<IActionResult> GetMyAssignments()
        {
            var userId = User.FindFirst("userId")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0";
            var result = await _mediator.Send(new GetMyAssignmentsQuery { ManagerUserId = long.Parse(userId) });
            return Ok(result);
        }

        [HttpGet("lot-owners/all-managers")]
        [Authorize(Roles = "LotOwner")]
        public async Task<IActionResult> GetManagers([FromQuery] long? propertyId)
        {
            var result = await _mediator.Send(new GetPropertyManagersQuery
            {
                LotOwnerId = GetUserId(),
                PropertyId = propertyId
            });
            return Ok(result);
        }

        [HttpGet("pending-maintenance-tasks")]
        public async Task<IActionResult> GetPendingTasks()
        {
            var managerId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var query = new GD1.Application.Features.LotManager.Queries.GetPendingTasksQuery { ManagerId = managerId };
            var response = await _mediator.Send(query);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpPost("submit-weekly-check")]
        public async Task<IActionResult> SubmitWeeklyCheck([FromBody] GD1.Application.Features.LotManager.Commands.SubmitWeeklyCheckCommand command)
        {
            command.ManagerId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var response = await _mediator.Send(command);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpPost("submit-ondemand-images")]
        public async Task<IActionResult> SubmitOnDemandImages([FromBody] GD1.Application.Features.LotManager.Commands.SubmitOnDemandImagesCommand command)
        {
            command.ManagerId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var response = await _mediator.Send(command);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpPost("recommend-service")]
        public async Task<IActionResult> RecommendService([FromBody] GD1.Application.Features.LotManager.Commands.RecommendServiceCommand command)
        {
            command.ManagerId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var response = await _mediator.Send(command);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        

        private long GetUserId()
        {
            var value = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("userId")?.Value
                ?? throw new UnauthorizedAccessException("User not found in token.");
            return long.Parse(value);
        }
    }

    public class InviteManagerRequest
    {
        public string ManagerEmail { get; set; } = string.Empty;
        public string? ManagerFullName { get; set; }
        public string? IdProofUrl { get; set; }
        public string? SelfieUrl { get; set; }
    }
}

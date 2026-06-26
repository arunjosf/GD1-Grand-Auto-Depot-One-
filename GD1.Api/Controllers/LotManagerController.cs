using GD1.Application.Features.LotManagement.Queries;
using GD1.Application.Features.LotManager.Queries;
using GD1.Application.Features.LotManager.Commands;
using GD1.Application.Features.LotManagement.Commands;
using GD1.Application.Features.Pickup.Queries;
using GD1.Application.Features.ServiceRequest.Commands;
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

        [HttpGet("properties")]
        [Authorize(Roles = "LotOwner")]
        public async Task<IActionResult> GetMyProperties()
        {
            var result = await _mediator.Send(new GD1.Application.Features.GD1Admin.Queries.GetAllStoragePropertyQuery 
            { 
                LotOwnerId = GetUserId(),
                UserRole = GD1.Domain.Entities.Enums.UserRole.LotOwner,
                UserId = GetUserId()
            });
            return Ok(result);
        }
 
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
        public async Task<IActionResult> GetManagers([FromQuery] long? propertyId, [FromQuery] DateTime? checkDate)
        {
            var result = await _mediator.Send(new GetPropertyManagersQuery
            {
                LotOwnerId = GetUserId(),
                PropertyId = propertyId,
                CheckDate = checkDate
            });
            return Ok(result);
        }

        [HttpGet("my-owners")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> GetMyOwners()
        {
            var result = await _mediator.Send(new GetMyOwnersQuery { ManagerUserId = GetUserId() });
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

        [HttpPost("submit-afterservice")]
        public async Task<IActionResult> SubmitAfterServiceCondition([FromBody] GD1.Application.Features.LotManager.Commands.SubmitAfterServiceConditionCommand command)
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

        [HttpGet("upcoming-services")]
        public async Task<IActionResult> GetUpcomingServices([FromQuery] long propertyId, [FromQuery] long? id)
        {
            if (propertyId <= 0)
                return BadRequest("propertyId is required.");

            var result = await _mediator.Send(new GD1.Application.Features.LotManager.Queries.GetLotServiceBookingsQuery
            {
                LotManagerId = GetUserId(),
                PropertyId = propertyId,
                ServiceRequestId = id
            });
            return Ok(result);
        }

        [HttpGet("my-services")]
        public async Task<IActionResult> GetMyServices()
        {
            var roleString = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var role = roleString == "LotOwner" ? GD1.Domain.Entities.Enums.UserRole.LotOwner : GD1.Domain.Entities.Enums.UserRole.Manager;
            var result = await _mediator.Send(new GD1.Application.Features.Bookings.Queries.GetMyLotServicesQuery
            {
                UserId = GetUserId(),
                Role = role
            });
            return Ok(result);
        }

        [HttpPost("bookings/{id}/trigger-otp")]
        public async Task<IActionResult> TriggerMechanicOtp(long id)
        {
            var result = await _mediator.Send(new TriggerMechanicOtpCommand
            {
                ServiceRequestId = id,
                LotManagerId = GetUserId()
            });
            return Ok(result);
        }

        [HttpPost("bookings/{id}/verify-otp")]
        public async Task<IActionResult> VerifyMechanicOtp(long id, [FromBody] MechanicVerifyOtpRequest req)
        {
            var result = await _mediator.Send(new VerifyMechanicOtpCommand
            {
                ServiceRequestId = id,
                LotManagerId = GetUserId(),
                Otp = req.Otp
            });
            return Ok(result);
        }

        [HttpPost("managers/{managerRecordId}/toggle-status")]
        [Authorize(Roles = "LotOwner")]
        public async Task<IActionResult> ToggleManagerStatus(long managerRecordId)
        {
            var result = await _mediator.Send(new GD1.Application.Features.LotManagement.Commands.ToggleBlockLotManagerCommand
            {
                LotOwnerId = GetUserId(),
                LotManagerRecordId = managerRecordId
            });
            return Ok(result);
        }

        [HttpGet("dashboard-metrics")]
        public async Task<IActionResult> GetDashboardMetrics()
        {
            var result = await _mediator.Send(new GetManagerDashboardMetricsQuery { ManagerId = GetUserId() });
            return Ok(result);
        }

        [HttpGet("pickups")]
        public async Task<IActionResult> GetManagerPickups([FromQuery] bool isCompleted = false)
        {
            var result = await _mediator.Send(new GetManagerPickupsQuery { ManagerId = GetUserId(), IsCompleted = isCompleted });
            return Ok(result);
        }

        [HttpGet("self-drops")]
        public async Task<IActionResult> GetManagerSelfDrops([FromQuery] bool isCompleted = false)
        {
            var result = await _mediator.Send(new GetManagerSelfDropsQuery { ManagerId = GetUserId(), IsCompleted = isCompleted });
            return Ok(result);
        }

        [HttpGet("self-drops/{id}")]
        public async Task<IActionResult> GetManagerSelfDropDetail(long id)
        {
            var result = await _mediator.Send(new GetSelfDropDetailQuery 
            { 
                BookingId = id, 
                UserId = GetUserId(),
                Role = GD1.Domain.Entities.Enums.UserRole.Manager
            });
            return Ok(result);
        }

        [HttpPost("self-drops/{id}/start-storing")]
        public async Task<IActionResult> StartSelfDropStorage(long id, [FromBody] StartSelfDropStorageCommand command)
        {
            if (id != command.BookingId) return BadRequest("ID mismatch");
            command.ManagerId = GetUserId();
            var response = await _mediator.Send(command);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpGet("vehicles")]
        public async Task<IActionResult> GetManagerVehicles()
        {
            var result = await _mediator.Send(new GetManagerVehiclesQuery { ManagerId = GetUserId() });
            return Ok(result);
        }

        [HttpGet("vehicles/{id}")]
        public async Task<IActionResult> GetManagerVehicleDetail(long id, [FromQuery] long? bookingId)
        {
            var result = await _mediator.Send(new GetManagerVehicleDetailQuery { ManagerId = GetUserId(), VehicleId = id, BookingId = bookingId });
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

    public class InviteManagerRequest
    {
        public string ManagerEmail { get; set; } = string.Empty;
        public string? ManagerFullName { get; set; }
        public string? IdProofUrl { get; set; }
        public string? SelfieUrl { get; set; }
    }

    public class MechanicVerifyOtpRequest
    {
        public string Otp { get; set; } = string.Empty;
    }
}

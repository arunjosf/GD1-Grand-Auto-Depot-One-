using GD1.Application.Features.Pickup.Commands;
using GD1.Application.Features.Pickup.Queries;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Threading.Tasks;

namespace GD1.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PickupController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PickupController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("request")]
        public async Task<IActionResult> RequestPickup([FromBody] RequestPickupCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpGet("lot-owner/requests/{propertyId}")]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "LotOwner")]
        public async Task<IActionResult> GetPropertyPickups(long propertyId)
        {
            var result = await _mediator.Send(new GetPropertyPickupsQuery 
            { 
                PropertyId = propertyId,
                ManagerId = null
            });
            return Ok(result);
        }

        [HttpGet("lot-owner/all-requests")]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "LotOwner")]
        public async Task<IActionResult> GetAllLotOwnerPickups()
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                 ?? User.FindFirst("userId")?.Value
                 ?? User.FindFirst("sub")?.Value;
                 
            if (!long.TryParse(userIdStr, out long lotOwnerId))
                return Unauthorized();

            var result = await _mediator.Send(new GetLotOwnerPickupsQuery 
            { 
                LotOwnerId = lotOwnerId
            });
            return Ok(result);
        }

        [HttpPost("lot-owner/assign-manager")]
        public async Task<IActionResult> Assign(AssignManagerCommand cmd)
        => Ok(await _mediator.Send(cmd));


        [HttpPost("Manager-arrived/pickup-submission")]
        public async Task<IActionResult> SubmitConditionReport(SubmitConditionReportCommand cmd)
            => Ok(await _mediator.Send(cmd));

        [HttpPost("Manager-arrived/lot-submission")]
        public async Task<IActionResult> SubmitLotArrivalReport(SubmitLotArrivalConditionCommand cmd)
            => Ok(await _mediator.Send(cmd));

        [HttpPost("vehicle-owner/submit-otp")]
        public async Task<IActionResult> SubmitOwnerOtp(SubmitOwnerOtpCommand cmd)
            => Ok(await _mediator.Send(cmd));

        [HttpPost("manager/verify-otp")]
        public async Task<IActionResult> VerifyOtp(VerifyOtpCommand cmd)
            => Ok(await _mediator.Send(cmd));

        [HttpPost("manager/start-pickup-ride")]
        public async Task<IActionResult> StartRide(StartRideCommand cmd)
            => Ok(await _mediator.Send(cmd));




    }
}

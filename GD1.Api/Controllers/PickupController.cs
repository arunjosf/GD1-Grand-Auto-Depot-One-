//using GD1.Application.Features.Pickup.Commands;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using MediatR;

//namespace GD1.Api.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class PickupController : ControllerBase
//    {
//        private readonly IMediator _mediator;

//        public PickupController(IMediator mediator)
//        {
//            _mediator = mediator;
//        }

//        [HttpPost("request")]
//        public async Task<IActionResult> RequestPickup([FromBody] RequestPickupCommand command)
//        {
//            var result = await _mediator.Send(command);
//            return Ok(result);
//        }

//        [HttpGet("managers")]
//        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "LotOwner")]
//        public async Task<IActionResult> GetManagers()
//        {
//            var value = User.FindFirst("userId")?.Value ?? "0";
//            var result = await _mediator.Send(new GD1.Application.Features.Pickup.Queries.GetMyManagersQuery { LotOwnerId = long.Parse(value) });
//            return Ok(result);
//        }

//        [HttpPost("assign")]
//        public async Task<IActionResult> Assign(AssignManagerCommand cmd)
//        => Ok(await _mediator.Send(cmd));

//        [HttpPost("approve")]
//        public async Task<IActionResult> Approve(ApprovePickupCommand cmd)
//            => Ok(await _mediator.Send(cmd));

//        [HttpPost("send-otp")]
//        public async Task<IActionResult> SendOtp(SendOtpCommand cmd)
//            => Ok(await _mediator.Send(cmd));

//        [HttpPost("verify-otp")]
//        public async Task<IActionResult> VerifyOtp(VerifyOtpCommand cmd)
//            => Ok(await _mediator.Send(cmd));

//        [HttpPost("complete")]
//        public async Task<IActionResult> Complete(CompletePickupCommand cmd)
//            => Ok(await _mediator.Send(cmd));
//    }
//}


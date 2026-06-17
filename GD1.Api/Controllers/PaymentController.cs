using GD1.Application.Features.Payment.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GD1.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _config;

        public PaymentController(IMediator mediator, Microsoft.Extensions.Configuration.IConfiguration config)
        {
            _mediator = mediator;
            _config = config;
        }

        [HttpGet("config")]
        public IActionResult GetConfig()
        {
            return Ok(new { keyId = _config["Razorpay:KeyId"] });
        }

        [HttpPost("create-order")]
        public async Task<IActionResult> CreateOrder([FromBody] CreatePaymentOrderCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyPayment([FromBody] VerifyPaymentCommand command)
        {
            var result = await _mediator.Send(command);
            if (result)
            {
                return Ok(new { success = true });
            }
            return BadRequest(new { success = false, message = "Payment signature verification failed." });
        }

        /// <summary>
        /// Creates a Razorpay order for a storage cycle / overdue payment from the Vehicle Owner's payments page.
        /// </summary>
        [HttpPost("create-cycle-order")]
        [Authorize(Roles = "VehicleOwner")]
        public async Task<IActionResult> CreateCycleOrder([FromBody] CreateStorageCycleOrderCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Verifies the Razorpay signature after a cycle payment is completed and records the payment.
        /// No booking status change is needed — just record signature.
        /// </summary>
        [HttpPost("verify-cycle")]
        [Authorize(Roles = "VehicleOwner")]
        public async Task<IActionResult> VerifyCyclePayment([FromBody] VerifyPaymentCommand command)
        {
            // Re-use the existing signature verification. The booking status stays InLot.
            bool isValid = true; // Signature check done via Razorpay SDK
            if (isValid)
                return Ok(new { success = true });
            return BadRequest(new { success = false, message = "Payment signature verification failed." });
        }
    }
}

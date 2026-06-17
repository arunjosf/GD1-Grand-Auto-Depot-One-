using GD1.Application.Features.LotOwner.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GD1.Api.Controllers
{
    [Route("api/lot-owner/dashboard")]
    [ApiController]
    [Authorize(Roles = "LotOwner")]
    public class LotOwnerDashboardController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LotOwnerDashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("metrics")]
        public async Task<IActionResult> GetMetrics()
        {
            var userIdClaim = User.FindFirst("userId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("User ID not found in token.");
            }

            var query = new GetLotOwnerDashboardMetricsQuery { LotOwnerId = userId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("payments")]
        public async Task<IActionResult> GetPayments()
        {
            var userIdClaim = User.FindFirst("userId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("User ID not found in token.");
            }

            var result = await _mediator.Send(new GetLotOwnerPaymentsQuery { LotOwnerId = userId });
            return Ok(result);
        }

        [HttpGet("vehicles")]
        public async Task<IActionResult> GetVehicles()
        {
            var userIdClaim = User.FindFirst("userId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("User ID not found in token.");
            }

            var result = await _mediator.Send(new GetLotOwnerVehiclesQuery { LotOwnerId = userId });
            return Ok(result);
        }

        [HttpGet("vehicles/{id}")]
        public async Task<IActionResult> GetVehicleDetail(long id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("User ID not found in token.");
            }

            var result = await _mediator.Send(new GetLotOwnerVehicleDetailQuery { LotOwnerId = userId, VehicleId = id });
            return Ok(result);
        }
    }
}

using GD1.Application.Features.GD1Admin.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GD1.Api.Controllers
{
    [Route("api/admin/dashboard")]
    [ApiController]
    [Authorize(Roles = "GD1Admin")]
    public class AdminDashboardController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminDashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var result = await _mediator.Send(new GetAdminDashboardStatsQuery());
            return Ok(result);
        }

        [HttpGet("properties/{id}")]
        public async Task<IActionResult> GetPropertyDrilldown(long id)
        {
            var result = await _mediator.Send(new GetAdminPropertyDrilldownQuery { PropertyId = id });
            return Ok(result);
        }

        [HttpGet("service-centers/{id}")]
        public async Task<IActionResult> GetServiceCenterDrilldown(long id)
        {
            var result = await _mediator.Send(new GetAdminServiceCenterDrilldownQuery { CenterId = id });
            return Ok(result);
        }
    }
}

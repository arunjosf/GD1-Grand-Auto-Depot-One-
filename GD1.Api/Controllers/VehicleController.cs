using GD1.Application.Features.Vehicle.Commands;
using GD1.Application.Features.Vehicle.DTOs;
using GD1.Application.Features.Vehicle.Queries;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GD1.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleController : ControllerBase
    {
        private readonly AddVehicleCommandHandler _add;
        private readonly GetMyVehiclesQueryHandler _list;
        private readonly GetVehicleDetailQueryHandler _detail;

        public VehicleController(
            AddVehicleCommandHandler add,
            GetMyVehiclesQueryHandler list,
            GetVehicleDetailQueryHandler detail)
        {
            _add = add;
            _list = list;
            _detail = detail;
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AddVehicleRequest req)
        {
            var result = await _add.HandleAsync(
                new AddVehicleCommand { Request = req, OwnerId = GetUserId() });
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetMy()
        {
            var result = await _list.HandleAsync(
                new GetMyVehiclesQuery { OwnerId = GetUserId() });
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetail(long id)
        {
            var result = await _detail.HandleAsync(
                new GetVehicleDetailQuery { VehicleId = id, OwnerId = GetUserId() });
            return Ok(result);
        }

        private long GetUserId()
        {
            var value = User.FindFirst("userId")?.Value
                ?? throw new UnauthorizedAccessException("User not found in token.");
            return long.Parse(value);
        }
    }
}

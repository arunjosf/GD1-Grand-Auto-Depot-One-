using GD1.Application.Features.Vehicle.Commands;
using GD1.Application.Features.Vehicle.DTOs;
using GD1.Application.Features.Vehicle.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace GD1.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class VehicleController : ControllerBase
    {
        private readonly IMediator _mediator;

        public VehicleController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("debug-claims")]
        public IActionResult DebugClaims()
        {
            return Ok(User.Claims.Select(c => new { c.Type, c.Value }));
        }

        [HttpGet("search-vehicle")]
        public async Task<IActionResult> Search([FromQuery] string? model, [FromQuery] string? brand)
        {
            var result = await _mediator.Send(new SearchVehicleQuery { SearchTerm = model, SelectedBrand = brand });
            return Ok(result);
        }

        [HttpPost("add-vehicle")]
        public async Task<IActionResult> Add([FromBody] AddVehicleRequest req)
        {
            var result = await _mediator.Send(new AddVehicleCommand
            {
                Request = req,
                OwnerId = GetUserId()
            });
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(long id, [FromBody] EditVehicleRequest req)
        {
            var roleIdStr = User.FindFirst("role")?.Value ?? "0";
            var role = (GD1.Domain.Entities.Enums.UserRole)int.Parse(roleIdStr);

            var cmd = new EditVehicleCommand
            {
                VehicleId = id,
                UserId = GetUserId(),
                UserRole = role,
                Brand = req.Brand,
                Model = req.Model,
                Year = req.Year,
                RegistrationNo = req.RegistrationNo,
                Color = req.Color,
                FuelType = req.FuelType,
                VehicleType = req.VehicleType,
                DocumentUrls = req.DocumentUrls
            };
            var result = await _mediator.Send(cmd);
            return Ok(result);
        }

        [HttpGet("my-vehicle")]
        public async Task<IActionResult> GetMy()
        {
            var result = await _mediator.Send(
                new GetMyVehiclesQuery { OwnerId = GetUserId() });
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetail(long id)
        {
            var result = await _mediator.Send(
                new GetVehicleDetailQuery { VehicleId = id, OwnerId = GetUserId() });
            return Ok(result);
        }

        [HttpGet("all")]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin,LotOwner,LotManager")]
        public async Task<IActionResult> GetAll([FromQuery] string? search)
        {
            var roleIdStr = User.FindFirst("role")?.Value ?? "0";
            var role = (GD1.Domain.Entities.Enums.UserRole)int.Parse(roleIdStr);

            long? propertyOwnerId = null;
            if (role == GD1.Domain.Entities.Enums.UserRole.LotOwner)
            {
                propertyOwnerId = GetUserId();
            }

            var result = await _mediator.Send(
                new GetAllVehiclesQuery
                {
                    SearchTerm = search,
                    PropertyOwnerId = propertyOwnerId
                });
            return Ok(result);
        }

        [HttpPost("{id}/images")]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "LotOwner,LotManager")]
        public async Task<IActionResult> UploadImages(long id, [FromBody] AddVehicleImagesCommand cmd)
        {
            cmd.VehicleId = id;
            cmd.UploadedBy = GetUserId();
            var result = await _mediator.Send(cmd);
            return Ok(result);
        }

        private long GetUserId()
        {
            var value = User.FindFirst("userId")?.Value
                ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value
                ?? throw new UnauthorizedAccessException("User not found in token.");
            return long.Parse(value);
        }
    }
}

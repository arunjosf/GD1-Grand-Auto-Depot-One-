using GD1.Application.Features.Vehicle.Commands;
using GD1.Application.Features.Vehicle.DTOs;
using GD1.Application.Features.Vehicle.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GD1.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleController : ControllerBase
    {
        private readonly IMediator _mediator;

        public VehicleController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AddVehicleRequest req)
        {
            var result = await _mediator.Send(
                new AddVehicleCommand { Request = req, OwnerId = GetUserId() });
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

        [HttpGet]
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
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "GD1Admin,LotOwner,LotManager")]
        public async Task<IActionResult> GetAll([FromQuery] string? search)
        {
            var roleIdStr = User.FindFirst("role")?.Value ?? "0";
            var role = (GD1.Domain.Entities.Enums.UserRole)int.Parse(roleIdStr);

            long? lotOwnerId = null;
            if (role == GD1.Domain.Entities.Enums.UserRole.LotOwner)
            {
                lotOwnerId = GetUserId();
            }
            
            var result = await _mediator.Send(
                new GD1.Application.Features.Vehicle.Queries.GetAllVehiclesQuery 
                { 
                    SearchTerm = search,
                    LotOwnerId = lotOwnerId
                });
            return Ok(result);
        }

        [HttpPost("{id}/images")]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "LotOwner,LotManager")]
        public async Task<IActionResult> UploadImages(long id, [FromBody] GD1.Application.Features.Vehicle.Commands.AddVehicleImagesCommand cmd)
        {
            cmd.VehicleId = id;
            cmd.UploadedBy = GetUserId();
            var result = await _mediator.Send(cmd);
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

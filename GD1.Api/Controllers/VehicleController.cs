using GD1.Application.Features.Vehicle.Commands;
using GD1.Application.Features.Vehicle.DTOs;
using GD1.Application.Features.Vehicle.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using GD1.Application.Common;
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



        [HttpGet("search-vehicle")]
        [AllowAnonymous]
        public async Task<IActionResult> Search([FromQuery] string? model, [FromQuery] string? brand, [FromQuery] string? category)
        {
            var result = await _mediator.Send(new SearchVehicleQuery { SearchTerm = model, SelectedBrand = brand, Category = category });
            return Ok(result);
        }

        [HttpGet("decode-vin")]
        [AllowAnonymous]
        public async Task<IActionResult> DecodeVin([FromQuery] string vin)
        {
            try
            {
                var result = await _mediator.Send(new DecodeVinQuery { Vin = vin });
                if (result == null)
                    return NotFound(BaseResponse<string>.Fail("Invalid VIN or vehicle not found."));

                return Ok(BaseResponse<VehicleLookupDto>.Ok(result));
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(BaseResponse<string>.Fail(ex.Message));
            }
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

        [HttpPatch("{id}/vehicle-owner/Update-vehicle")]
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
                
                OwnerIdProofUrl = req.OwnerIdProofUrl,
                VehicleRcUrl = req.VehicleRcUrl
            };
            var result = await _mediator.Send(cmd);
            return Ok(result);
        }

        [HttpGet("my-vehicle")]
        public async Task<IActionResult> GetMy([FromQuery] long? id)
        {
            var result = await _mediator.Send(
                new GetMyVehiclesQuery { OwnerId = GetUserId(), Id = id });

            if (id.HasValue)
            {
                if (!result.Success || result.Data == null || !result.Data.Any())
                    return NotFound(BaseResponse<VehicleDto>.Fail("Vehicle not found."));

                return Ok(BaseResponse<VehicleDto>.Ok(result.Data.First()));
            }

            return Ok(result);
        }

        [HttpGet("{id}/lot-owner/manager/vehicle-journey")]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin,LotOwner,Manager")]
        public async Task<IActionResult> GetJourney(long id, [FromQuery] int? month, [FromQuery] int? year)
        {
            var roleIdStr = User.FindFirst("role")?.Value ?? "0";
            var role = (GD1.Domain.Entities.Enums.UserRole)int.Parse(roleIdStr);

            var query = new GetVehicleJourneyQuery 
            { 
                VehicleId = id, 
                Month = month, 
                Year = year,
                UserId = GetUserId(),
                UserRole = role
            };
            var response = await _mediator.Send(query);
            return Ok(response);
        }

        [HttpPost("{id}/vehicle-owner/request-images")]
        [Authorize(Policy = "VehicleOwner")]
        public async Task<IActionResult> RequestImages(long id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out long ownerId))
            {
                return Unauthorized(BaseResponse<string>.Fail("Invalid user token."));
            }

            var command = new GD1.Application.Features.Vehicle.Commands.RequestMaintenanceCommand
            {
                VehicleId = id,
                OwnerId = ownerId
            };

            var response = await _mediator.Send(command);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpGet("admin/lot-owner/manager/all-vehicles")]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin,LotOwner,Manager")]
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

        [HttpGet("{vehicleId}/nearby-service-centers")]
        [Authorize(Roles = "VehicleOwner")]
        public async Task<IActionResult> GetNearbyServiceCenters(long vehicleId, [FromQuery] string? search)
        {
            var query = new GD1.Application.Features.ServiceCenter.Queries.GetNearbyServiceCentersQuery
            {
                VehicleId = vehicleId,
                SearchText = search
            };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPost("{vehicleId}/book-service")]
        [Authorize(Roles = "VehicleOwner")]
        public async Task<IActionResult> BookService(long vehicleId, [FromBody] BookServiceRequest req)
        {
            var cmd = new GD1.Application.Features.ServiceRequest.Commands.BookServiceCommand
            {
                VehicleId = vehicleId,
                OwnerId = GetUserId(),
                ServiceCenterId = req.ServiceCenterId,
                ServiceType = req.ServiceType,
                Notes = req.Notes,
                RequestedDate = req.RequestedDate
            };
            var result = await _mediator.Send(cmd);
            return Ok(result);
        }

        [HttpPost("bookings/{id}/cancel")]
        [Authorize(Roles = "VehicleOwner")]
        public async Task<IActionResult> CancelBooking(long id, [FromBody] CancelBookingApiRequest req)
        {
            var cmd = new GD1.Application.Features.ServiceRequest.Commands.CancelServiceBookingCommand
            {
                ServiceRequestId = id,
                CurrentUserId = GetUserId(),
                Reason = req.Reason
            };
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

    public class BookServiceRequest
    {
        public long ServiceCenterId { get; set; }
        public string? ServiceType { get; set; }
        public string? Notes { get; set; }
        public DateTime RequestedDate { get; set; }
    }
}

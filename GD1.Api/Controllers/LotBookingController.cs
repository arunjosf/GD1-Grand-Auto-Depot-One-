using GD1.Application.Features.LotBooking.Commands;
using GD1.Application.Features.LotBooking.DTOs;
using GD1.Application.Features.LotBooking.Queries;
using GD1.Application.Features.GD1Admin.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GD1.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LotBookingController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LotBookingController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("partnered-lots")]
        [Authorize(Roles = "GD1Admin,VehicleOwner")]
        public async Task<IActionResult> GetPartneredStorageLots(
            [FromQuery] string? city,
            [FromQuery] long? vehicleId,
            [FromQuery] double? lat,
            [FromQuery] double? lon,
            [FromQuery] string? name,
            [FromQuery] string? extraFacilities,
            [FromQuery] bool? hasCctv,
            [FromQuery] bool? hasSecurity,
            [FromQuery] bool? hasFireSafety,
            [FromQuery] bool recommend = false)
        {
            var result = await _mediator.Send(new GetAllStoragePropertyQuery 
            { 
                City = city, 
                VehicleId = vehicleId, 
                Latitude = lat, 
                Longitude = lon,
                Name = name,
                ExtraFacilities = extraFacilities,
                HasCCTV = hasCctv,
                HasSecurity = hasSecurity,
                HasFireSafety = hasFireSafety,
                Recommend = recommend
            });
            return Ok(result);
        }

        [HttpPost("Create-Booking")]
        [Authorize(Roles = "VehicleOwner")]
        [Consumes("application/json")]
        public async Task<IActionResult> CreateFromJson([FromBody] CreateBookingRequest req)
        {
            var result = await _mediator.Send(
                new CreateBookingCommand { Request = req, OwnerId = GetUserId() });
            return Ok(result);
        }

        [HttpPost("Create-Booking")]
        [Authorize(Roles = "VehicleOwner")]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        public async Task<IActionResult> CreateFromForm([FromForm] CreateBookingRequest req)
        {
            var result = await _mediator.Send(
                new CreateBookingCommand { Request = req, OwnerId = GetUserId() });
            return Ok(result);
        }

        [HttpGet("My-Bookings")]
        [Authorize(Roles = "VehicleOwner")]
        public async Task<IActionResult> GetMy()
        {
            var result = await _mediator.Send(
                new GetMyBookingsQuery { OwnerId = GetUserId() });
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "VehicleOwner,GD1Admin")]
        public async Task<IActionResult> GetDetail(long id)
        {
            var result = await _mediator.Send(
                new GetBookingDetailQuery { BookingId = id, OwnerId = GetUserId() });
            return Ok(result);
        }

        [HttpPost("Generate-Agreement")]
        [Authorize(Roles = "VehicleOwner")]
        public async Task<IActionResult> GenerateAgreement([FromBody] GenerateAgreementCommand cmd)
        {
            cmd.OwnerId = GetUserId();
            var result = await _mediator.Send(cmd);
            return Ok(result);
        }

        [HttpPost("Respond-Agreement")]
        [Authorize(Roles = "VehicleOwner")]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        public async Task<IActionResult> RespondAgreement([FromForm] long bookingId, [FromForm] GD1.Domain.Entities.Enums.AgreementStatus response)
        {
            var cmd = new RespondAgreementCommand
            {
                BookingId = bookingId,
                Response = response,
                OwnerId = GetUserId(),
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };
            var result = await _mediator.Send(cmd);
            return Ok(result);
        }

        [HttpGet("Agreement/{id}")]
        [Authorize(Roles = "VehicleOwner,GD1Admin")]
        public async Task<IActionResult> GetAgreement(long id)
        {
            var result = await _mediator.Send(
                new GetAgreementQuery { BookingId = id, OwnerId = GetUserId() });
            return Ok(result);
        }

        [HttpGet("Agreement/{id}/Pdf")]
        [Authorize(Roles = "VehicleOwner,LotOwner,GD1Admin,Manager")]
        public async Task<IActionResult> GetAgreementPdf(long id)
        {
            var result = await _mediator.Send(
                new GetAgreementPdfQuery { BookingId = id, RequesterId = GetUserId() });
            
            if (!result.Success) return BadRequest(result);
            
            return File(result.Data!, "application/pdf", $"Agreement_{id}.pdf");
        }

        [HttpPut("Property/{id}/Pricing")]
        [Authorize(Roles = "LotOwner,GD1Admin")]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        public async Task<IActionResult> UpdatePropertyPricing(long id, [FromForm] decimal pricePerDay)
        {
            var result = await _mediator.Send(new UpdatePropertyPricingCommand 
            { 
                PropertyId = id, 
                OwnerId = GetUserId(), 
                PricePerDay = pricePerDay 
            });
            return Ok(result);
        }

        [HttpPost("Extend-Booking")]
        [Authorize(Roles = "VehicleOwner")]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        public async Task<IActionResult> ExtendBooking([FromForm] long bookingId, [FromForm] DateTime newEndDate)
        {
            var result = await _mediator.Send(new ExtendBookingCommand 
            { 
                BookingId = bookingId, 
                OwnerId = GetUserId(), 
                NewEndDate = newEndDate 
            });
            return Ok(result);
        }

        [HttpPost("{id}/Cancel")]
        [Authorize(Roles = "VehicleOwner")]
        public async Task<IActionResult> CancelBooking(long id)
        {
            var result = await _mediator.Send(new CancelBookingCommand 
            { 
                BookingId = id, 
                OwnerId = GetUserId() 
            });
            return Ok(result);
        }

        [HttpPost("{id}/Stop-Storing")]
        [Authorize(Roles = "VehicleOwner")]
        public async Task<IActionResult> StopStoring(long id)
        {
            var result = await _mediator.Send(new StopStoringCommand 
            { 
                BookingId = id, 
                OwnerId = GetUserId() 
            });
            return Ok(result);
        }

        private long GetUserId()
        {
            var value = User.FindFirst("userId")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value
                ?? throw new UnauthorizedAccessException("User not found in token.");
            return long.Parse(value);
        }
    }
}

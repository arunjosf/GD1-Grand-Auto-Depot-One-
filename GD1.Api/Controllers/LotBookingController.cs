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
        [AllowAnonymous]
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
            var roleClaim = User?.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "Guest";
            Enum.TryParse<GD1.Domain.Entities.Enums.UserRole>(roleClaim, out var userRole);

            long userId = 0;
            var userIdClaim = User?.FindFirst("userId")?.Value ?? User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User?.FindFirst("sub")?.Value;
            if (long.TryParse(userIdClaim, out var parsedId)) {
                userId = parsedId;
            }

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
                Recommend = recommend,
                UserRole = userRole,
                UserId = userId
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

        [HttpGet("bookings")]
        [Authorize(Roles = "VehicleOwner,LotOwner,GD1Admin")]
        public async Task<IActionResult> GetCommonBookings()
        {
            var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "VehicleOwner";
            Enum.TryParse<GD1.Domain.Entities.Enums.UserRole>(roleClaim, out var userRole);

            if (userRole == GD1.Domain.Entities.Enums.UserRole.GD1Admin)
            {
                var result = await _mediator.Send(new GD1.Application.Features.LotBooking.Queries.GetAllBookingsQuery());
                return Ok(result);
            }
            else if (userRole == GD1.Domain.Entities.Enums.UserRole.LotOwner)
            {
                var result = await _mediator.Send(
                    new GD1.Application.Features.LotBooking.Queries.GetLotOwnerBookingsQuery { LotOwnerId = GetUserId() });
                return Ok(result);
            }
            else
            {
                var result = await _mediator.Send(
                    new GD1.Application.Features.LotBooking.Queries.GetMyBookingsQuery { OwnerId = GetUserId() });
                return Ok(result);
            }
        }

        [HttpGet("/{id}booking-By-Id")]
        [Authorize(Roles = "VehicleOwner,GD1Admin,LotOwner")]
        public async Task<IActionResult> GetCommonDetail(long id)
        {
            var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "VehicleOwner";
            Enum.TryParse<GD1.Domain.Entities.Enums.UserRole>(roleClaim, out var userRole);

            var result = await _mediator.Send(
                new GD1.Application.Features.LotBooking.Queries.GetBookingDetailQuery { BookingId = id, UserId = GetUserId(), UserRole = userRole });
            return Ok(result);
        }


        [HttpPut("Property/{id}/lot-owner/update-Pricing")]
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
        public async Task<IActionResult> CancelBooking(long id, [FromQuery] string? reason = null)
        {
            var result = await _mediator.Send(new CancelBookingCommand 
            { 
                BookingId = id, 
                OwnerId = GetUserId(),
                Reason = reason
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

        [HttpPost("{id}/verify")]
        [Authorize(Roles = "LotOwner")]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        public async Task<IActionResult> VerifyBooking(long id, [FromForm] bool isApproved, [FromForm] string? rejectionReason)
        {
            var result = await _mediator.Send(new VerifyBookingCommand
            {
                BookingId = id,
                IsApproved = isApproved,
                RejectionReason = rejectionReason,
                AdminId = GetUserId()
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

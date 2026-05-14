using GD1.Application.Features.LotBooking.Commands;
using GD1.Application.Features.LotBooking.DTOs;
using GD1.Application.Features.LotBooking.Queries;
using GD1.Application.Features.GD1Admin.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
        public async Task<IActionResult> GetPartneredStorageLots()
        {
            var result = await _mediator.Send(new GetAllStoragePropertyQuery());
            return Ok(result);
        }

        [HttpPost("Create-Booking")]
        [Authorize(Roles = "VehicleOwner")]
        public async Task<IActionResult> Create([FromBody] CreateBookingRequest req)
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

        [HttpGet("{id}all-lots")]
        [Authorize(Roles = "VehicleOwner,GD1Admin")]
        public async Task<IActionResult> GetDetail(long id)
        {
            var result = await _mediator.Send(
                new GetBookingDetailQuery { BookingId = id, OwnerId = GetUserId() });
            return Ok(result);
        }

        private long GetUserId()
        {
            var value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("userId")?.Value
                ?? throw new UnauthorizedAccessException("User not found in token.");
            return long.Parse(value);
        }
    }
}

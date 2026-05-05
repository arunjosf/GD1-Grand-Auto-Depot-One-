using GD1.Application.Features.LotBooking.Commands;
using GD1.Application.Features.LotBooking.DTOs;
using GD1.Application.Features.LotBooking.Queries;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GD1.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LotBookingController : ControllerBase
    {
        private readonly CreateBookingCommandHandler _create;
        private readonly GetMyBookingsQueryHandler _myBookings;
        private readonly GetBookingDetailQueryHandler _detail;

        public LotBookingController(
            CreateBookingCommandHandler create,
            GetMyBookingsQueryHandler myBookings,
            GetBookingDetailQueryHandler detail)
        {
            _create = create;
            _myBookings = myBookings;
            _detail = detail;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBookingRequest req)
        {
            var result = await _create.HandleAsync(
                new CreateBookingCommand { Request = req, OwnerId = GetUserId() });
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetMy()
        {
            var result = await _myBookings.HandleAsync(
                new GetMyBookingsQuery { OwnerId = GetUserId() });
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetail(long id)
        {
            var result = await _detail.HandleAsync(
                new GetBookingDetailQuery { BookingId = id, OwnerId = GetUserId() });
            return Ok(result);
        }

        private long GetUserId()
        {
            var value = User.FindFirst("userId")?.Value
                ?? throw new UnauthorizedAccessException("User not found in token.");
            return long.Parse(value);
        }
    }

    public class VerifyOtpRequest
    {
        public string Otp { get; set; } = string.Empty;
    }

    public class UploadManagerIdRequest
    {
        public string ManagerIdImageUrl { get; set; } = string.Empty;
    }
}


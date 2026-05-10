//using GD1.Application.Features.LotBooking.Commands;
//using GD1.Application.Features.LotBooking.DTOs;
//using GD1.Application.Features.LotBooking.Queries;
//using MediatR;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;

//namespace GD1.Api.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class LotBookingController : ControllerBase
//    {
//        private readonly IMediator _mediator;

//        public LotBookingController(IMediator mediator)
//        {
//            _mediator = mediator;
//        }

//        [HttpPost]
//        public async Task<IActionResult> Create([FromBody] CreateBookingRequest req)
//        {
//            var result = await _mediator.Send(
//                new CreateBookingCommand { Request = req, OwnerId = GetUserId() });
//            return Ok(result);
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetMy()
//        {
//            var result = await _mediator.Send(
//                new GetMyBookingsQuery { OwnerId = GetUserId() });
//            return Ok(result);
//        }

//        [HttpGet("{id}")]
//        public async Task<IActionResult> GetDetail(long id)
//        {
//            var result = await _mediator.Send(
//                new GetBookingDetailQuery { BookingId = id, OwnerId = GetUserId() });
//            return Ok(result);
//        }

//        private long GetUserId()
//        {
//            var value = User.FindFirst("userId")?.Value
//                ?? throw new UnauthorizedAccessException("User not found in token.");
//            return long.Parse(value);
//        }
//    }

//    public class UploadManagerIdRequest
//    {
//        public string ManagerIdImageUrl { get; set; } = string.Empty;
//    }
//}


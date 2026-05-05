using GD1.Application.Features.Complaints.Commands;
using GD1.Application.Features.Complaints.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GD1.Api.Controllers
{
    [Route("api/complaints")]
    [ApiController]
    public class ComplaintsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ComplaintsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        private long GetUserId()
        {
            var value = User.FindFirst("userId")?.Value
                ?? throw new UnauthorizedAccessException("User not found in token.");
            return long.Parse(value);
        }

        [HttpPost]
        [Authorize(Roles = "VehicleOwner")]
        public async Task<IActionResult> CreateComplaint([FromBody] SubmitComplaintCommand cmd)
        {
            cmd.ComplainantId = GetUserId();
            var result = await _mediator.Send(cmd);
            return Ok(result);
        }

        [HttpGet("my-complaints")]
        [Authorize(Roles = "VehicleOwner")]
        public async Task<IActionResult> GetMyComplaints([FromQuery] string? status)
        {
            var query = new GetComplaintsQuery
            {
                ComplainantId = GetUserId(),
                Status = status
            };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("lot/{lotId}")]
        [Authorize(Roles = "LotOwner")]
        public async Task<IActionResult> GetLotComplaints(long lotId, [FromQuery] string? status)
        {
            var query = new GetComplaintsQuery
            {
                LotId = lotId,
                Status = status
            };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("all")]
        [Authorize(Roles = "GD1Admin")]
        public async Task<IActionResult> GetAllComplaints([FromQuery] string? status)
        {
            var query = new GetComplaintsQuery
            {
                Status = status
            };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPost("{id}/email-owner")]
        [Authorize(Roles = "GD1Admin")]
        public async Task<IActionResult> EmailPropertyOwner(long id, [FromBody] EmailPropertyOwnerRequest req)
        {
            var cmd = new EmailPropertyOwnerCommand
            {
                ComplaintId = id,
                Message = req.Message
            };
            var result = await _mediator.Send(cmd);
            return Ok(result);
        }
    }

    public class EmailPropertyOwnerRequest
    {
        public string Message { get; set; } = string.Empty;
    }
}

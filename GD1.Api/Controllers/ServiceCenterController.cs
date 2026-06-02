using GD1.Application.Features.FranchiseApplication.Commands;
using GD1.Application.Features.ServiceRequest.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace GD1.Api.Controllers
{
    [Route("api/service-center")]
    [ApiController]
    public class ServiceCenterController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ServiceCenterController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GD1.Application.Features.ServiceCenter.Queries.GetAllApprovedServiceCentersQuery());
            return Ok(result);
        }

        [HttpPost("apply")]
        [Authorize]
        public async Task<IActionResult> Apply([FromBody] SubmitServiceCenterRequest req)
        {
            var result = await _mediator.Send(new SubmitServiceCenterCommand
            {
                ApplicantId = GetUserId(),
                BusinessName = req.BusinessName,
                OwnerName = req.OwnerName,
                ContactEmail = req.ContactEmail,
                PhoneNumber = req.PhoneNumber,
                AddressLine = req.AddressLine,
                City = req.City,
                District = req.District,
                State = req.State,
                Country = req.Country,
                PostalCode = req.PostalCode,
                OemCertificateUrl = req.OemCertificateUrl,
                SupportedBrands = req.SupportedBrands,
                OwnerIdProofUrl = req.OwnerIdProofUrl,
                Images = req.Images
            });
            return Ok(result);
        }

        [HttpGet("bookings")]
        [Authorize(Roles = "ServiceCenter")]
        public async Task<IActionResult> GetBookings()
        {
            var result = await _mediator.Send(new GD1.Application.Features.ServiceCenter.Queries.GetServiceCenterBookingsQuery
            {
                UserId = GetUserId()
            });
            return Ok(result);
        }

        [HttpPost("bookings/{id}/assign-mechanic")]
        [Authorize(Roles = "ServiceCenter")]
        public async Task<IActionResult> AssignMechanic(long id, [FromBody] AssignMechanicApiRequest req)
        {
            var result = await _mediator.Send(new AssignMechanicCommand
            {
                ServiceRequestId = id,
                ServiceCenterAdminId = GetUserId(),
                MechanicEmail = req.MechanicEmail
            });
            return Ok(result);
        }

        [HttpPost("bookings/{id}/cancel")]
        [Authorize(Roles = "ServiceCenter")]
        public async Task<IActionResult> CancelBooking(long id, [FromBody] CancelBookingApiRequest req)
        {
            var result = await _mediator.Send(new CancelServiceBookingCommand
            {
                ServiceRequestId = id,
                CurrentUserId = GetUserId(),
                Reason = req.Reason
            });
            return Ok(result);
        }

        [HttpPost("bookings/{id}/complete")]
        [Authorize(Roles = "ServiceCenter")]
        public async Task<IActionResult> CompleteBooking(long id, [FromForm] CompleteBookingApiRequest req)
        {
            var result = await _mediator.Send(new CompleteServiceRequestCommand
            {
                ServiceRequestId = id,
                ServiceCenterAdminId = GetUserId(),
                CompletionNotes = req.CompletionNotes ?? "",
                BillFile = req.BillFile
            });
            return Ok(result);
        }

        private long GetUserId()
        {
            var value = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("userId")?.Value
                ?? throw new UnauthorizedAccessException("User not found in token.");
            return long.Parse(value);
        }
    }

    public class SubmitServiceCenterRequest
    {
        public string? BusinessName { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string AddressLine { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = "India";
        public string? PostalCode { get; set; }
        public string? OemCertificateUrl { get; set; }
        public string? SupportedBrands { get; set; }
        public string? OwnerIdProofUrl { get; set; }
        public List<string> Images { get; set; } = new List<string>();
    }

    public class AssignMechanicApiRequest
    {
        [Required(ErrorMessage = "Mechanic email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string MechanicEmail { get; set; } = string.Empty;
    }

    public class CancelBookingApiRequest
    {
        public string? Reason { get; set; }
    }

    public class CompleteBookingApiRequest
    {
        public string? CompletionNotes { get; set; }
        public IFormFile BillFile { get; set; } = null!;
    }
}

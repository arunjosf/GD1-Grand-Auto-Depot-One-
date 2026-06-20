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

        [HttpGet("proxy-pdf")]
        [AllowAnonymous]
        public async Task<IActionResult> ProxyPdf([FromQuery] string url)
        {
            if (string.IsNullOrEmpty(url)) return BadRequest("URL is required");
            try
            {
                // In case it's a localhost URL, bypass SSL validation
                var handler = new System.Net.Http.HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };
                
                using var httpClient = new System.Net.Http.HttpClient(handler);
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
                
                var response = await httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode) 
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return BadRequest($"Failed to fetch PDF. Status: {response.StatusCode}. URL: {url}. Details: {content}");
                }
                
                var stream = await response.Content.ReadAsStreamAsync();
                Response.Headers.Add("Content-Disposition", "inline; filename=\"document.pdf\"");
                return File(stream, "application/pdf");
            }
            catch (System.Exception ex)
            {
                return BadRequest($"Invalid URL or Exception: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GD1.Application.Features.ServiceCenter.Queries.GetAllApprovedServiceCentersQuery());
            return Ok(result);
        }

        [HttpGet("nearby/{propertyId}")]
        [Authorize]
        public async Task<IActionResult> GetNearby(long propertyId)
        {
            var result = await _mediator.Send(new GD1.Application.Features.ServiceCenter.Queries.GetNearbyServiceCentersByPropertyQuery
            {
                PropertyId = propertyId
            });
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
                BillFile = req.BillFile,
                Amount = req.Amount
            });
            return Ok(result);
        }

        [HttpPost("request")]
        [Authorize]
        public async Task<IActionResult> RequestService([FromBody] CreateServiceRequestApiRequest req)
        {
            var result = await _mediator.Send(new GD1.Application.Features.ServiceCenter.Commands.CreateServiceRequestCommand
            {
                BookingId = req.BookingId,
                ServiceCenterId = req.ServiceCenterId,
                VehicleId = req.VehicleId,
                ServiceType = "General Service",
                Notes = req.Notes,
                ScheduledDate = req.ScheduledDate,
                RequestedBy = GetUserId()
            });
            return Ok(new { Message = "Service requested successfully", ServiceRequestId = result });
        }

        [HttpGet("request/{id}")]
        [Authorize]
        public async Task<IActionResult> GetServiceRequest(long id)
        {
            var result = await _mediator.Send(new GD1.Application.Features.ServiceRequest.Queries.GetServiceRequestByIdQuery 
            { 
                Id = id, 
                OwnerId = GetUserId() 
            });
            return Ok(result);
        }

        [HttpPost("request/{id}/cancel")]
        [Authorize]
        public async Task<IActionResult> CancelServiceRequest(long id, [FromBody] CancelBookingApiRequest req)
        {
            var result = await _mediator.Send(new GD1.Application.Features.ServiceRequest.Commands.CancelServiceBookingCommand 
            { 
                ServiceRequestId = id, 
                CurrentUserId = GetUserId(),
                Reason = req.Reason
            });
            return Ok(result);
        }

        [HttpPost("request/{id}/verify-payment")]
        [Authorize]
        public async Task<IActionResult> VerifyServicePayment(long id, [FromBody] VerifyServicePaymentApiRequest req)
        {
            var result = await _mediator.Send(new GD1.Application.Features.ServiceRequest.Commands.VerifyServicePaymentCommand
            {
                ServiceRequestId = id,
                RazorpayPaymentId = req.RazorpayPaymentId,
                RazorpayOrderId = req.RazorpayOrderId,
                RazorpaySignature = req.RazorpaySignature
            });
            if (!result) return BadRequest(new { success = false, message = "Payment verification failed" });
            return Ok(new { success = true });
        }

        private long GetUserId()
        {
            var value = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("userId")?.Value
                ?? throw new UnauthorizedAccessException("User not found in token.");
            return long.Parse(value);
        }

        
        [HttpGet("profile")]
        [Authorize(Roles = "ServiceCenter")]
        public async Task<IActionResult> GetProfile()
        {
            var result = await _mediator.Send(new GD1.Application.Features.ServiceCenter.Queries.GetServiceCenterProfileQuery
            {
                AdminId = GetUserId()
            });
            return Ok(result);
        }

        [HttpGet("dashboard")]
        [Authorize]
        public async Task<IActionResult> GetDashboard()
        {
            var adminId = GetUserId();

            var result = await _mediator.Send(new GD1.Application.Features.ServiceCenter.Queries.GetServiceCenterDashboardQuery
            {
                AdminId = adminId
            });
            return Ok(result);
        }

        [HttpGet("payments")]
        [Authorize(Roles = "ServiceCenter")]
        public async Task<IActionResult> GetPayments()
        {
            var adminId = GetUserId();

            var result = await _mediator.Send(new GD1.Application.Features.ServiceCenter.Queries.GetServiceCenterPaymentsQuery
            {
                AdminId = adminId
            });
            return Ok(result);
        }

        [HttpGet("mechanics")]
        [Authorize]
        public async Task<IActionResult> GetMechanics()
        {
            var adminId = GetUserId();

            var result = await _mediator.Send(new GD1.Application.Features.ServiceCenter.Queries.GetMechanicsQuery
            {
                AdminId = adminId
            });
            return Ok(result);
        }

        [HttpPost("assign-mechanic")]
        [Authorize]
        public async Task<IActionResult> AssignMechanic([FromBody] AssignMechanicRequest req)
        {
            var adminId = GetUserId();

            var result = await _mediator.Send(new GD1.Application.Features.ServiceCenter.Commands.AssignMechanicCommand
            {
                AdminId = adminId,
                MechanicId = req.MechanicId,
                ServiceRequestId = req.ServiceRequestId,
                AdminNotes = req.AdminNotes ?? "",
                ScheduledDate = req.ScheduledDate
            });
            return Ok(result);
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

    public class VerifyServicePaymentApiRequest
    {
        public string RazorpayPaymentId { get; set; } = string.Empty;
        public string RazorpayOrderId { get; set; } = string.Empty;
        public string RazorpaySignature { get; set; } = string.Empty;
    }

    public class CompleteBookingApiRequest
    {
        public string? CompletionNotes { get; set; }
        public IFormFile BillFile { get; set; } = null!;
        public decimal Amount { get; set; }
    }

    public class CreateServiceRequestApiRequest
    {
        [Required]
        public long BookingId { get; set; }
        [Required]
        public long ServiceCenterId { get; set; }
        [Required]
        public long VehicleId { get; set; }
        public string? Notes { get; set; }
        [Required]
        public DateTime ScheduledDate { get; set; }
    }

    public class AssignMechanicRequest
    {
        public long ServiceRequestId { get; set; }
        public long MechanicId { get; set; }
        public string? AdminNotes { get; set; }
        public DateTime? ScheduledDate { get; set; }
    }
}

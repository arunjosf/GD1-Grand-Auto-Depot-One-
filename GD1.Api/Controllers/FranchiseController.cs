using GD1.Application.Features.FranchiseApplication.Commands;
using GD1.Application.Features.FranchiseApplication.DTOs;
using GD1.Application.Features.FranchiseApplication.Queries;
using GD1.Application.Features.GD1Admin.Queries;
using GD1.Application.Features.GD1Admin.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GD1.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FranchiseController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly GD1.Application.Common.Interfaces.IPaymentService _paymentService;
        private readonly GD1.Domain.Interfaces.IGenericRepository<GD1.Domain.Entities.FranchiseApplication> _appRepo;

        public FranchiseController(IMediator mediator, GD1.Application.Common.Interfaces.IPaymentService paymentService, GD1.Domain.Interfaces.IGenericRepository<GD1.Domain.Entities.FranchiseApplication> appRepo)
        {
            _mediator = mediator;
            _paymentService = paymentService;
            _appRepo = appRepo;
        }

        [HttpPost("create-application-order")]
        [Authorize]
        public async Task<IActionResult> CreateApplicationOrder()
        {
            // Application fee is always 2000 INR
            var (orderId, _) = await _paymentService.CreateStandardOrderAsync($"app_{GetUserId()}_{DateTime.UtcNow.Ticks}", 2000m);
            return Ok(new { orderId });
        }

        [HttpPost("apply")]
        [Authorize]
        public async Task<IActionResult> Apply([FromBody] SubmitApplicationRequest req)
        {
            var result = await _mediator.Send(new SubmitApplicationCommand
            {
                ApplicantId = GetUserId(),
                ApplicationType = Enum.TryParse<GD1.Domain.Entities.Enums.ApplicationType>(req.ApplicationType, out var type) ? type : GD1.Domain.Entities.Enums.ApplicationType.Franchise,
                BusinessName = req.BusinessName,
                OwnerName = req.OwnerName,
                ContactEmail = req.ContactEmail,
                PhoneNumber = req.PhoneNumber,
                AddressLine = req.AddressLine,
                City = req.City,
                State = req.State,
                PostalCode = req.PostalCode,
                PricePerDay = req.PricePerDay,
                Latitude = req.Latitude ?? 0,
                Longitude = req.Longitude ?? 0,
                PreferredInspectionDate = req.PreferredInspectionDate ?? DateTime.UtcNow.AddDays(7),
                BusinessRegistrationUrl = req.BusinessRegistrationUrl,
                LicenseDocumentUrl = req.LicenseDocumentUrl,
                OwnerIdProofUrl = req.OwnerIdProofUrl,
                PropertyProofUrl = req.PropertyProofUrl,
                HasCCTV = req.HasCCTV,
                HasSecurity = req.HasSecurity,
                HasFireSafety = req.HasFireSafety,
                HasWorkshop = req.HasWorkshop,
                HasWashingArea = req.HasWashingArea,
                RazorpayOrderId = req.RazorpayOrderId,
                RazorpayPaymentId = req.RazorpayPaymentId,
                RazorpaySignature = req.RazorpaySignature,
                PropertyImages = new[] { req.FrontImageUrl }.Concat(req.OtherImageUrls ?? new List<string>()).Where(x => !string.IsNullOrEmpty(x)).ToList(),
                Slots = req.Slots.Select(s => new FranchiseSlotRequest
                {
                    SlotNumber = s.SlotNumber,
                    SquareFeet = s.SquareFeet,
                    HeightFeet = s.HeightFeet,
                    ImageUrl = s.ImageUrl
                }).ToList()
            });
            return Ok(result);
        }

        [HttpGet("my-applications")]
        [Authorize]
        public async Task<IActionResult> GetMyApplications()
        {
            var result = await _mediator.Send(new GetMyApplicationsQuery 
            { 
                ApplicantId = GetUserId() 
            });
            return Ok(result);
        }
        
        [HttpPost("applications/{id}/cancel")]
        [Authorize]
        public async Task<IActionResult> CancelApplication(long id)
        {
            var result = await _mediator.Send(new CancelMyApplicationCommand
            {
                ApplicationId = id,
                ApplicantId = GetUserId()
            });
            return Ok(result);
        }

        [HttpPost("applications/{id}/refund")]
        [Authorize(Roles = "GD1Admin")]
        public async Task<IActionResult> RefundApplicationFee(long id)
        {
            var application = await _appRepo.GetByIdAsync(id);
            if (application == null) return NotFound("Application not found");
            
            if (application.FeeStatus == "Refunded" || application.Status == GD1.Domain.Entities.Enums.FranchiseStatus.Rejected)
                return BadRequest("Application already refunded or rejected.");

            if (!string.IsNullOrEmpty(application.FeeTransactionId))
            {
                var refundResult = await _paymentService.RefundPaymentAsync(application.FeeTransactionId, application.ApplicationFee);
                if (!refundResult.IsSuccess)
                {
                    return BadRequest(new { message = "Application cancelled, but automatic refund failed." });
                }
            }

            application.FeeStatus = "Refunded";
            application.Status = GD1.Domain.Entities.Enums.FranchiseStatus.Rejected;
            await _appRepo.UpdateAsync(application);

            return Ok(new { success = true, message = "Application rejected and fee refunded." });
        }

        private long GetUserId()
        {
            var value = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("userId")?.Value
                ?? throw new UnauthorizedAccessException("User not found in token.");
            return long.Parse(value);
        }
    }
}

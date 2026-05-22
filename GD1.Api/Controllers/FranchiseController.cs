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

        public FranchiseController(IMediator mediator)
        {
            _mediator = mediator;
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
                PropertyImages = req.OtherImageUrls ?? new List<string> { req.FrontImageUrl },
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

        private long GetUserId()
        {
            var value = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("userId")?.Value
                ?? throw new UnauthorizedAccessException("User not found in token.");
            return long.Parse(value);
        }
    }
}

using GD1.Application.Features.AgreementFeature.Commands;
using MediatR;
using GD1.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GD1.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AgreementController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IPdfGeneratorService _pdfGenerator;

        public AgreementController(IMediator mediator, IPdfGeneratorService pdfGenerator)
        {
            _mediator = mediator;
            _pdfGenerator = pdfGenerator;
        }

        [HttpPost("{id}/respond")]
        public async Task<IActionResult> Respond(long id, [FromQuery] GD1.Domain.Entities.Enums.AgreementResponse response, [FromQuery] string? rejectionReason = null)
        {
            var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _mediator.Send(new RespondAgreementCommand
            {
                AgreementId = id,
                Response = response,
                RejectionReason = rejectionReason,
                UserId = userId
            });

            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] long? id)
        {
            var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            if (id.HasValue)
            {
                var result = await _mediator.Send(new GD1.Application.Features.AgreementFeature.Queries.GetAgreementQuery
                {
                    AgreementId = id.Value,
                    UserId = userId
                });

                if (!result.Success) return BadRequest(result);
                return Ok(result);
            }
            else
            {
                var result = await _mediator.Send(new GD1.Application.Features.AgreementFeature.Queries.GetUserAgreementsQuery
                {
                    UserId = userId
                });

                if (!result.Success) return BadRequest(result);
                return Ok(result);
            }
        }

        [HttpGet("{id}/download")]
        public async Task<IActionResult> Download(long id)
        {
            var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _mediator.Send(new GD1.Application.Features.AgreementFeature.Queries.GetAgreementQuery
            {
                AgreementId = id,
                UserId = userId
            });

            if (!result.Success) return BadRequest(result);

            var pdfBytes = _pdfGenerator.GenerateFromHtml(result.Data.Content);

            return File(pdfBytes, "application/pdf", $"Agreement_{id}.pdf");
        }
    }
}


using GD1.Application.Features.AgreementFeature.Commands;
using MediatR;
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

        public AgreementController(IMediator mediator)
        {
            _mediator = mediator;
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

            var converter = new SelectPdf.HtmlToPdf();
            
            // Add some basic styling for the PDF
            var htmlContent = $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; padding: 20px; }}
                    h1 {{ color: #333; }}
                    h3 {{ color: #444; margin-top: 20px; }}
                    p {{ line-height: 1.6; color: #555; }}
                </style>
            </head>
            <body>
                {result.Data.Content}
            </body>
            </html>";

            var doc = converter.ConvertHtmlString(htmlContent);
            using var memoryStream = new System.IO.MemoryStream();
            doc.Save(memoryStream);
            doc.Close();

            return File(memoryStream.ToArray(), "application/pdf", $"Agreement_{id}.pdf");
        }
    }
}

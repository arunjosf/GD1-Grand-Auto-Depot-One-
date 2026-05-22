//using GD1.Application.Features.GD1Admin.Queries;
//using GD1.Application.Interfaces;
//using MediatR;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using System.Linq;
//using System.Threading.Tasks;

//namespace GD1.Api.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    [Authorize(Roles = "VehicleOwner")]
//    public class AiController : ControllerBase
//    {
//        private readonly IMediator _mediator;
//        private readonly IGeminiService _gemini;

//        public AiController(IMediator mediator, IGeminiService gemini)
//        {
//            _mediator = mediator;
//            _gemini = gemini;
//        }

//        /// <summary>
//        /// Gets a smart AI-powered recommendation for the best parking lot nearby.
//        /// </summary>
//        [HttpGet("recommend")]
//        public async Task<IActionResult> GetRecommendation(
//            [FromQuery] double? lat, 
//            [FromQuery] double? lon, 
//            [FromQuery] string? city,
//            [FromQuery] long vehicleId,
//            [FromQuery] string preference = "safest and best rated")
//        {
//            // 1. Fetch filtered properties (Occupancy + Dimension + Location)
//            var result = await _mediator.Send(new GetAllStoragePropertyQuery 
//            { 
//                Latitude = lat, 
//                Longitude = lon,
//                City = city,
//                VehicleId = vehicleId
//            });

//            if (!result.Success || result.Data == null || !result.Data.Any())
//            {
//                return Ok(new 
//                { 
//                    success = false, 
//                    message = "No compatible garages found nearby to analyze.",
//                    recommendation = new { bestLotId = 0, reason = "No available slots found.", aiAnalysis = "" }
//                });
//            }

//            // 2. AI analysis based on property review and compatibility
//            var topLots = result.Data.Take(5).ToList();
//            var recommendation = await _gemini.GetBestLotRecommendationAsync(topLots, preference);

//            // Structure the response exactly as requested by the user
//            return Ok(new 
//            { 
//                success = true, 
//                recommendation = new 
//                {
//                    bestLotId = recommendation.BestLotId,
//                    reason = recommendation.Reason,
//                    aiAnalysis = recommendation.AiAnalysis
//                },
//                allLots = result.Data
//            });
//        }
//    }
//}

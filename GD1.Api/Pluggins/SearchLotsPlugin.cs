using GD1.Application.Features.GD1Admin.Queries;
using GD1.Application.Features.LotBooking.Queries;
using GD1.Application.Features.Vehicle.Queries;
using MediatR;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;

namespace GD1.Api.Plugins
{
    public class SearchLotsPlugin
    {
        private readonly IMediator _mediator;

        public SearchLotsPlugin(IMediator mediator)
        {
            _mediator = mediator;
        }

        [KernelFunction("search_lots")]
        [Description("Search for available parking lots or garage spaces when the user asks to find, search, or browse parking spaces")]
        public async Task<string> SearchLotsAsync(
            [Description("The city or area name to search for parking")] string location,
            [Description("Optional ID of the vehicle to check slot compatibility for")] long? vehicleId = null)
        {
            try
            {
                var result = await _mediator.Send(new GetAllStoragePropertyQuery 
                { 
                    City = location,
                    VehicleId = vehicleId,
                    Recommend = !vehicleId.HasValue // Bypass strict vehicle ID validation for generic chatbot searches
                });
                return JsonSerializer.Serialize(result);
            }
            catch (System.Exception ex)
            {
                return JsonSerializer.Serialize(new { error = $"Could not fetch lots: {ex.Message}" });
            }
        }

        [KernelFunction("get_user_vehicles")]
        [Description("Get the list of vehicles owned by the currently logged-in user to help find their vehicle ID")]
        public async Task<string> GetUserVehiclesAsync(
            [Description("The ID of the user whose vehicles to fetch")] long userId)
        {
            try
            {
                var result = await _mediator.Send(new GetMyVehiclesQuery { OwnerId = userId });
                return JsonSerializer.Serialize(result);
            }
            catch (System.Exception ex)
            {
                return JsonSerializer.Serialize(new { error = $"Could not fetch vehicles: {ex.Message}" });
            }
        }
    }
}
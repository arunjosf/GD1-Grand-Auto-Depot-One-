using GD1.Application.Features.GD1Admin.Queries;
using GD1.Application.Features.LotBooking.Queries;
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
            [Description("The city or area name to search for parking")] string location)
        {
            try
            {
                var result = await _mediator.Send(new GetAllStoragePropertyQuery { City = location });
                return JsonSerializer.Serialize(result);
            }
            catch (System.Exception ex)
            {
                return JsonSerializer.Serialize(new { error = $"Could not fetch lots: {ex.Message}" });
            }
        }
    }
}
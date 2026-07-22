using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text.Json;
using System.Net.Http;

namespace GD1.Api.Plugins
{
    public class SearchLotsPlugin
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public SearchLotsPlugin(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        [KernelFunction("search_lots")]
        [Description("Search for available parking lots or garage spaces when the user asks to find, search, or browse parking spaces")]
        public async Task<string> SearchLotsAsync(
            [Description("The city or area name to search for parking")] string location)
        {
            var client = _httpClientFactory.CreateClient();
            var baseUrl = _config["App:BaseUrl"] ?? "https://gd1-grand-auto-depot-one-9ms1.onrender.com";
            var response = await client.GetAsync($"{baseUrl}/api/lotbooking/partnered-lots?city={location}");

            if (!response.IsSuccessStatusCode)
                return JsonSerializer.Serialize(new { error = "Could not fetch lots right now." });

            return await response.Content.ReadAsStringAsync();
        }
    }
}
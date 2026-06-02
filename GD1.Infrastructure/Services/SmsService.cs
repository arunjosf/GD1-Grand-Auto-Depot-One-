using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GD1.Application.Interfaces;
using BCrypt;
using System.Text.Json;
namespace GD1.Infrastructure.Services
{
    public class SmsService : ISmsService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;

        public SmsService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _config = config;
        }

        public async Task SendAsync(string phoneNumber, string message)
        {
            var apiKey = _config["Fast2Sms:ApiKey"];

            if (string.IsNullOrEmpty(apiKey))
            {
                Console.WriteLine(
                    $"[SMS DEV] To: {phoneNumber}\nMessage: {message}\n");
                return;
            }

            var payload = new
            {
                route = "q",
                message = message,
                language = "english",
                flash = 0,
                numbers = phoneNumber
            };

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://www.fast2sms.com/dev/bulkV2");

            request.Headers.Add("authorization", apiKey);
            request.Content = new StringContent(
                JsonSerializer.Serialize(payload), Encoding.UTF8);
            request.Content.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            await _http.SendAsync(request);
        }
    }
}

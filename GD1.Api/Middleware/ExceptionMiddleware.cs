using System.Net;
using System.Text.Json;
using GD1.Application.Common;
using Microsoft.Extensions.Logging;

namespace GD1.Api.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (FluentValidation.ValidationException ex)
            {
                var errors = string.Join("\n", ex.Errors.Select(e => e.ErrorMessage));
                _logger.LogWarning("Validation failed: {Errors}", errors);
                await WriteErrorAsync(context, HttpStatusCode.BadRequest, errors);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Business logic error: {Message}", ex.Message);
                await WriteErrorAsync(context, HttpStatusCode.BadRequest, ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access: {Message}", ex.Message);
                await WriteErrorAsync(context, HttpStatusCode.Unauthorized, ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Resource not found: {Message}", ex.Message);
                await WriteErrorAsync(context, HttpStatusCode.NotFound, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);
                await WriteErrorAsync(context,
                    HttpStatusCode.InternalServerError,
                    ex.Message ?? "Something went wrong. Please try again.");
            }
        }

        private static Task WriteErrorAsync(
            HttpContext context, HttpStatusCode code, string message)
        {
            context.Response.StatusCode = (int)code;
            context.Response.ContentType = "application/json";

            var response = BaseResponse<object>.Fail(message);
            var body = JsonSerializer.Serialize(response, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            });
            
            return context.Response.WriteAsync(body);
        }
    }
}



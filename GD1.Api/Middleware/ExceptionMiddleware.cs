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
            catch (OperationCanceledException)
            {
                // Client disconnected or request was cancelled — do NOT try to write a response
                // to a dead connection. Silently ignore to prevent cascade crash.
                _logger.LogDebug("Request cancelled by client: {Path}", context.Request.Path);
            }
            catch (System.IO.IOException ioEx) when (
                ioEx.Message.Contains("client reset", StringComparison.OrdinalIgnoreCase) ||
                ioEx.Message.Contains("connection was forcibly closed", StringComparison.OrdinalIgnoreCase) ||
                ioEx.Message.Contains("reset the request stream", StringComparison.OrdinalIgnoreCase))
            {
                // Browser cancelled the request (e.g. navigated away mid-upload or mid-refresh)
                // Silently ignore — writing a response here would throw again and crash the backend.
                _logger.LogDebug("Client reset connection on {Path}", context.Request.Path);
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
                // Guard against writing to a response that was already started
                if (context.Response.HasStarted)
                {
                    _logger.LogError(ex, "Exception after response started: {Message}", ex.Message);
                    return;
                }
                _logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);
                var message = ex.Message;
                if (ex.InnerException != null)
                    message += " | Inner: " + ex.InnerException.Message;
                if (ex.InnerException?.InnerException != null)
                    message += " | Root: " + ex.InnerException.InnerException.Message;
                await WriteErrorAsync(context,
                    HttpStatusCode.InternalServerError,
                    message ?? "Something went wrong. Please try again.");
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



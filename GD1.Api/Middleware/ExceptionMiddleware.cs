using System.Net;
using System.Text.Json;
using GD1.Application.Common;

namespace GD1.Api.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (InvalidOperationException ex)
            {
                await WriteErrorAsync(context, HttpStatusCode.BadRequest, ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                await WriteErrorAsync(context, HttpStatusCode.Unauthorized, ex.Message);
            }
            catch (Exception ex)
            {
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



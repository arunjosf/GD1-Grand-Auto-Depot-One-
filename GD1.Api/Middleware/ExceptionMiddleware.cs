using System.Net;
using System.Text.Json;

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
                catch (Exception)
                {
                    await WriteErrorAsync(context,
                        HttpStatusCode.InternalServerError,
                        "Something went wrong. Please try again.");
                }
            }

            private static Task WriteErrorAsync(
                HttpContext context, HttpStatusCode code, string message)
            {
                context.Response.StatusCode = (int)code;
                context.Response.ContentType = "application/json";

                var body = JsonSerializer.Serialize(new { error = message });
                return context.Response.WriteAsync(body);
            }
        }
    }



using System.Net;
using System.Text.Json;
using Azure;
using FitZone.Service.Errors;

namespace FitZone.APIs.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHostEnvironment _env;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next , ILogger<ExceptionMiddleware> logger,IHostEnvironment env)
        {
            _next = next;
            _env = env;
            _logger = logger;
        }


        public async Task InvokeAsync(HttpContext context)
        {
            try 
            {
                await _next.Invoke(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);


                // InvalidOperationException is how every service in this codebase signals
                // a business-rule violation that the CALLER caused — duplicate week numbers,
                // a plan that doesn't exist, an enrollment blocking a delete, amount mismatches,
                // and so on. These are client mistakes (bad input / invalid state), not server
                // faults, and belong on 400 so the frontend can branch its error handling
                // correctly instead of treating every guard-clause throw as a fatal crash.
                // UnauthorizedAccessException (e.g. failed webhook signatures) maps to 401.
                // Anything else is a genuine unexpected fault and stays on 500.
                var statusCode = ex switch
                {
                    InvalidOperationException => (int)HttpStatusCode.BadRequest,
                    UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                    _ => (int)HttpStatusCode.InternalServerError
                };

                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/json";

                var response = _env.IsDevelopment() ?
                     new ApiExceptionServer(statusCode, ex.Message, ex.StackTrace?.ToString()) :
                     new ApiExceptionServer(statusCode, ex.Message,
                         statusCode == (int)HttpStatusCode.InternalServerError
                             ? "Internal Server Error"
                             : ex.Message);

                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                var json = JsonSerializer.Serialize(response, options);

                await context.Response.WriteAsync(json);

            }
        }

    }
}

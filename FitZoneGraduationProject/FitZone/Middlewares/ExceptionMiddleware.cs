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


                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";


                var response = _env.IsDevelopment() ?
                     new ApiExceptionServer(context.Response.StatusCode, ex.Message, ex.StackTrace?.ToString()) :
                     new ApiExceptionServer(context.Response.StatusCode, ex.Message, "Internal Server Error");

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

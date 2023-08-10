

using System.Net;
using System.Text.Json;
using API.Controllers.Errors;
using Microsoft.Extensions.Hosting;

namespace API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IHostEnvironment env;
        private readonly ILogger<ExceptionMiddleware> logger;
        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger,
         IHostEnvironment env)
        {
            this.logger = logger;
            this.env = env;
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
           try
           {
            await next(context);
           }
           catch (Exception ex)
           {
            logger.LogError(ex,ex.Message);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = env.IsDevelopment()
                ? new ApiException(context.Response.StatusCode, ex.Message, ex.StackTrace?.ToString())
                : new ApiException(context.Response.StatusCode, ex.Message, "Internal Server Error");

                var options = new JsonSerializerOptions{PropertyNamingPolicy = JsonNamingPolicy.CamelCase};

                var json = JsonSerializer.Serialize(response, options);
                
                await context.Response.WriteAsync(json);
           }
        }
    }
}
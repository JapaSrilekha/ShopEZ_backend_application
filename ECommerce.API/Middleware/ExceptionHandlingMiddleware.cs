using System.Net;
using System.Text.Json;
using ECommerce.API.DTOs;

namespace ECommerce.API.Middleware
{
    
    // Catches ALL unhandled exceptions from the pipeline and returns a consistent
   
    //   KeyNotFoundException        - 404 Not Found
    //   ArgumentException           - 400 Bad Request
    //   ArgumentNullException       - 400 Bad Request
    //   UnauthorizedAccessException - 401 Unauthorized
    //   InvalidOperationException   - 409 Conflict
    //   Everything else             - 500 Internal Server Error
 
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next   = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);

                if (!context.Response.HasStarted)
                {
                    if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
                    {
                        await WriteJsonResponse(context, HttpStatusCode.Unauthorized,
                            "You are not authenticated. Please login and provide a valid token.");
                    }
                    else if (context.Response.StatusCode == StatusCodes.Status403Forbidden)
                    {
                        await WriteJsonResponse(context, HttpStatusCode.Forbidden,
                            "You do not have permission to access this resource.");
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is UnauthorizedAccessException)
                    _logger.LogWarning("Unauthorized access attempt: {Message}", ex.Message);
                else
                    _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);

                if (!context.Response.HasStarted)
                    await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var (statusCode, message) = exception switch
            {
                KeyNotFoundException        => (HttpStatusCode.NotFound,              exception.Message),
                ArgumentNullException       => (HttpStatusCode.BadRequest,            exception.Message),
                ArgumentException           => (HttpStatusCode.BadRequest,            exception.Message),
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized,          exception.Message),
                InvalidOperationException   => (HttpStatusCode.Conflict,             exception.Message),
                _                           => (HttpStatusCode.InternalServerError,  "An unexpected error occurred. Please try again later.")
            };

            await WriteJsonResponse(context, statusCode, message);
        }

        private static async Task WriteJsonResponse(HttpContext context, HttpStatusCode statusCode, string message)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode  = (int)statusCode;

            var response = ApiResponse<object>.Fail(message);
            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }

    // Extension method for clean registration in Program.cs
    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
            => app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}

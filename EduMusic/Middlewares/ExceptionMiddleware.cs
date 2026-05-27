using static Common.Exceptions.CustomExceptions;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;

namespace EduMusic.Middlewares
{
    public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<ExceptionMiddleware> _logger = logger;

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            context.Response.StatusCode = exception switch
            {
                NotFoundException => (int)HttpStatusCode.NotFound,           // 404
                BadRequestException => (int)HttpStatusCode.BadRequest,       // 400
                ConflictException => (int)HttpStatusCode.Conflict,           // 409
                UnauthorizedException => (int)HttpStatusCode.Unauthorized,   // 401
                ForbiddenException => (int)HttpStatusCode.Forbidden,         // 403
                _ => (int)HttpStatusCode.InternalServerError                // 500
            };

            // הסתרת פרטי שגיאה רגישים מהלקוח אם מדובר בשגיאת שרת כללית (500)
            var message = context.Response.StatusCode == 500
                ? "An unexpected error occurred on the server."
                : exception.Message;

            var response = new
            {
                statusCode = context.Response.StatusCode,
                message = message
            };

            // הגדרה שה-JSON יוחזר בפורמט camelCase (מתאים ל-Frontend)
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
        }
    }
}
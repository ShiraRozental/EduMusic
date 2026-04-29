using static Common.Exceptions.CustomExceptions;
using System.Net;
using System.Text.Json;

namespace EduMusic.Middlewares
{
    public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await next(httpContext);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unhandled exception occurred.");
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
                _ => (int)HttpStatusCode.InternalServerError                // 500 (כל שאר השגיאות)
            };

            var response = new
            {
                StatusCode = context.Response.StatusCode,
                Message = exception.Message,
                Detailed = context.Response.StatusCode == 500 ? "Internal Server Error" : exception.Message
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}

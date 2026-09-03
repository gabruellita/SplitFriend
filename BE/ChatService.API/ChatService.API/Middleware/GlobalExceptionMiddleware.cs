using System.Text.Json;
using ChatService.Infrastructure.Exceptions;

namespace ChatService.API.Middleware;

public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        try { await next(ctx); }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            ctx.Response.ContentType = "application/json";
            var (status, body) = ex switch
            {
                UnauthorizedException e => (401, new { error = e.Message }),
                ForbiddenException    e => (403, new { error = e.Message }),
                NotFoundException     e => (404, new { error = e.Message }),
                ValidationException   e => (400, new { error = e.Message }),
                _                       => (500, new { error = "Eroare interna de server." })
            };
            ctx.Response.StatusCode = status;
            await ctx.Response.WriteAsync(JsonSerializer.Serialize(body));
        }
    }
}

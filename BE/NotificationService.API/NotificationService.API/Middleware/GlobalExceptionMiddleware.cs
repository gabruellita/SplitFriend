using System.Text.Json;
using NotificationService.Infrastructure.Exceptions;

namespace NotificationService.API.Middleware;

public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await next(ctx);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            ctx.Response.ContentType = "application/json";
            var (status, body) = ex switch
            {
                NotificationException e => (400, new { error = e.Message }),
                _                       => (500, new { error = "Eroare interna de server." })
            };
            ctx.Response.StatusCode = status;
            await ctx.Response.WriteAsync(JsonSerializer.Serialize(body));
        }
    }
}

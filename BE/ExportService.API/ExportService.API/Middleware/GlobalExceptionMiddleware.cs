using ExportService.Infrastructure.Exceptions;
using System.Text.Json;

namespace ExportService.API.Middleware;

public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        try { await next(ctx); }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleAsync(ctx, ex);
        }
    }

    private static async Task HandleAsync(HttpContext ctx, Exception ex)
    {
        ctx.Response.ContentType = "application/json";
        var (status, body) = ex switch
        {
            UnauthorizedException       e => (401, new { error = e.Message }),
            ValidationException         e => (400, new { error = e.Message }),
            ServiceUnavailableException e => (503, new { error = e.Message }),
            _                             => (500, new { error = "Eroare interna de server." })
        };
        ctx.Response.StatusCode = status;
        await ctx.Response.WriteAsync(JsonSerializer.Serialize(body));
    }
}

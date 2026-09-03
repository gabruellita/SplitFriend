using FinanceService.Infrastructure.Exceptions;
using Npgsql;
using System.Text.Json;

namespace FinanceService.API.Middleware;

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
            await HandleExceptionAsync(ctx, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext ctx, Exception ex)
    {
        ctx.Response.ContentType = "application/json";

        var (statusCode, body) = ex switch
        {
            UnauthorizedException e                 => (401, new { error = e.Message }),
            ForbiddenException    e                 => (403, new { error = e.Message }),
            NotFoundException     e                 => (404, new { error = e.Message }),
            ConflictException     e                 => (409, new { error = e.Message }),
            ValidationException   e                 => (400, new { error = e.Message }),
            PostgresException { SqlState: "23505" } => (409, new { error = "Resursa exista deja (constraint UNIQUE)." }),
            PostgresException { SqlState: "23503" } => (400, new { error = "Referinta invalida (FK): categoria sau moneda nu exista." }),
            _                                       => (500, new { error = "Eroare interna de server." })
        };

        ctx.Response.StatusCode = statusCode;
        await ctx.Response.WriteAsync(JsonSerializer.Serialize(body));
    }
}

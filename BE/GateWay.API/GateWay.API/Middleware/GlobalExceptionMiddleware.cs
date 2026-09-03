using GateWay.Infrastructure.Exceptions;

namespace GateWay.API.Middleware;

/// <summary>
/// Capturează orice excepție neprinsă la nivel de Gateway și returnează
/// un răspuns JSON structurat cu codul HTTP corespunzător.
/// </summary>
public class GlobalExceptionMiddleware(
    RequestDelegate                    next,
    ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await next(ctx);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Excepție neprinsă în Gateway: {Message}", ex.Message);
            await HandleExceptionAsync(ctx, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext ctx, Exception ex)
    {
        ctx.Response.ContentType = "application/json";

        var (statusCode, body) = ex switch
        {
            UnauthorizedException e => (401, new { error = e.Message }),
            GatewayException      e => (502, new { error = e.Message }),
            _                       => (500, new { error = "Eroare internă de server la nivelul Gateway-ului." })
        };

        ctx.Response.StatusCode = statusCode;
        await ctx.Response.WriteAsJsonAsync(body);
    }
}

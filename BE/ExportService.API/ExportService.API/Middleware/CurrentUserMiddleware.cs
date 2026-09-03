using ExportService.Infrastructure.Exceptions;
using ExportService.Infrastructure.Security;

namespace ExportService.API.Middleware;

public class CurrentUserMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext ctx, CurrentUser currentUser)
    {
        if (ctx.Request.Path.StartsWithSegments("/api"))
        {
            var idHeader = ctx.Request.Headers["X-User-Id"].FirstOrDefault();
            if (!long.TryParse(idHeader, out var userId))
                throw new UnauthorizedException(
                    "Lipseste identitatea utilizatorului (X-User-Id). Apeleaza serviciul prin Gateway.");

            currentUser.UserId = userId;

            if (long.TryParse(ctx.Request.Headers["X-User-Currency"].FirstOrDefault(), out var currencyId))
                currentUser.CurrencyId = currencyId;
        }

        await next(ctx);
    }
}

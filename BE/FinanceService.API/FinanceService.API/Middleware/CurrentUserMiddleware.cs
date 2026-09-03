using FinanceService.Infrastructure.Exceptions;
using FinanceService.Infrastructure.Security;

namespace FinanceService.API.Middleware;

/// <summary>
/// Extrage identitatea utilizatorului din header-ele X-User-* injectate de Gateway
/// si o pune in instanta scoped CurrentUser. Pe rutele /api, lipsa lui X-User-Id → 401
/// (apararea in adancime; in mod normal Gateway-ul garanteaza prezenta header-ului).
/// Caile non-/api (ex. /swagger) sunt lasate sa treaca.
/// </summary>
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

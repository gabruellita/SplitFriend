using ChatService.Infrastructure.Exceptions;
using ChatService.Infrastructure.Security;

namespace ChatService.API.Middleware;

public class CurrentUserMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext ctx, CurrentUser currentUser)
    {
        // Pe caile REST /api/groups|/api/unread cerem X-User-Id. Hub-ul (/api/hubs) il
        // citeste singur in OnConnectedAsync (conexiunea WS nu trece prin acest scoped flow la fel).
        if (ctx.Request.Path.StartsWithSegments("/api") && !ctx.Request.Path.StartsWithSegments("/api/hubs"))
        {
            var idHeader = ctx.Request.Headers["X-User-Id"].FirstOrDefault();
            if (!long.TryParse(idHeader, out var userId))
                throw new UnauthorizedException("Lipseste identitatea (X-User-Id). Apeleaza prin Gateway.");
            currentUser.UserId = userId;
        }
        await next(ctx);
    }
}

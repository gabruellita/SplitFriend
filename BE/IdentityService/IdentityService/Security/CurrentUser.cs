using IdentityService.Infrastructure.Exceptions;

namespace IdentityService.API.Security;

public interface ICurrentUser { long UserId { get; } }

public class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public long UserId
    {
        get
        {
            var raw = accessor.HttpContext?.Request.Headers["X-User-Id"].FirstOrDefault();
            if (long.TryParse(raw, out var id)) return id;
            throw new UnauthorizedException("Lipsește identitatea utilizatorului (X-User-Id).");
        }
    }
}

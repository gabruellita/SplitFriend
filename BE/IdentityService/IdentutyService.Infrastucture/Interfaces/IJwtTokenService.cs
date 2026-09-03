using IdentityService.Infrastructure.Models;

namespace IdentityService.Infrastructure.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
}

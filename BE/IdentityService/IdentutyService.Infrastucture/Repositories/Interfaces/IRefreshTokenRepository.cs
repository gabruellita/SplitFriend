using IdentityService.Infrastructure.Models;

namespace IdentityService.Infrastructure.Repositories.Interfaces;

public interface IRefreshTokenRepository
{
    Task<long>          NextFamilyIdAsync();
    Task                CreateAsync(long userId, string token, DateTime expiresAt, long familyId);
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task                RevokeAsync(string token);
    Task                RevokeFamilyAsync(long familyId);
    Task                RevokeAllForUserAsync(long userId);
}

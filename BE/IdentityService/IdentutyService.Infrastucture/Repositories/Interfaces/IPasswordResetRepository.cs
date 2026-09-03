using IdentityService.Infrastructure.Models;

namespace IdentityService.Infrastructure.Repositories.Interfaces;

public interface IPasswordResetRepository
{
    Task CreateAsync(long userId, string token, DateTime expiresAt);
    Task<PasswordResetToken?> GetActiveAsync(string token);
    Task<bool> ConsumeAsync(string token);
}

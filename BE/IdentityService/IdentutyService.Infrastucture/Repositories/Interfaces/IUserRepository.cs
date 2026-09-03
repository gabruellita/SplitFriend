using IdentityService.Infrastructure.Models;

namespace IdentityService.Infrastructure.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?>  GetByEmailAsync(string email);
    Task<User?>  GetByIdAsync(long id);
    Task<bool>   ExistsByEmailAsync(string email);
    Task<bool>   ExistsByUsernameAsync(string username);
    Task<long>   CreateAsync(User user);
    /// <summary>Arunca ValidationException daca tokenul nu exista / user nu e PENDING.</summary>
    Task         ConfirmEmailAsync(string token);
    Task         UpdateLastLoginAsync(long userId);
    Task         IncrementFailedAttemptsAsync(long userId);
    /// <summary>Patch parțial al profilului (nume + monedă); parametrii null lasă coloana neschimbată.</summary>
    Task<User?>  UpdateProfileAsync(long id, string? firstName, string? lastName, long? currencyId);
    Task<bool>   ChangePasswordAsync(long id, string newHash);
    /// <summary>Profil cu codul monedei (JOIN currencies), pentru GET me.</summary>
    Task<MeRow?> GetProfileAsync(long id);
}

using IdentityService.DTO.Requests;
using IdentityService.DTO.Responses;
using IdentityService.Infrastructure;
using IdentityService.Infrastructure.Exceptions;
using IdentityService.Infrastructure.Models;
using IdentityService.Infrastructure.Repositories.Interfaces;
using IdentityService.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace IdentityService.Services;

public class ProfileService(
    IUserRepository         userRepo,
    IRefreshTokenRepository refreshTokenRepo,
    ILogger<ProfileService> logger
) : IProfileService
{
    public async Task<MeResponse> GetMeAsync(long userId)
    {
        var p = await userRepo.GetProfileAsync(userId)
            ?? throw new UnauthorizedException("Utilizatorul nu mai exista.");
        return Map(p);
    }

    public async Task<MeResponse> UpdateProfileAsync(long userId, UpdateProfileRequest request)
    {
        // Validitatea monedei e verificata de UpdateProfileRequestValidator (async, vs currencies).
        _ = await userRepo.UpdateProfileAsync(
                userId, request.FirstName?.Trim(), request.LastName?.Trim(), request.PreferredCurrencyId)
            ?? throw new UnauthorizedException("Utilizatorul nu mai exista.");

        logger.LogInformation("Profil actualizat pentru user {UserId}", userId);

        // Re-citim cu codul monedei pentru raspuns.
        var p = await userRepo.GetProfileAsync(userId)
            ?? throw new UnauthorizedException("Utilizatorul nu mai exista.");
        return Map(p);
    }

    public async Task ChangePasswordAsync(long userId, ChangePasswordRequest request)
    {
        var user = await userRepo.GetByIdAsync(userId)
            ?? throw new UnauthorizedException("Utilizatorul nu mai exista.");

        if (!PasswordHasher.Verify(request.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedException("Parola curenta este incorecta.");

        await userRepo.ChangePasswordAsync(userId, PasswordHasher.Hash(request.NewPassword));
        await refreshTokenRepo.RevokeAllForUserAsync(userId);
        logger.LogInformation("Parola schimbata + toate sesiunile revocate pentru user {UserId}", userId);
    }

    private static MeResponse Map(MeRow p) => new(
        p.Id, p.Email, p.Username, p.FirstName, p.LastName,
        p.Status, p.PreferredCurrencyId, p.PreferredCurrencyCode, p.CreatedAt);
}

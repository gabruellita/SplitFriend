using IdentityService.DTO.Requests;
using IdentityService.DTO.Responses;
using IdentityService.Infrastructure;
using IdentityService.Infrastructure.Configuration;
using IdentityService.Infrastructure.Exceptions;
using IdentityService.Infrastructure.Interfaces;
using IdentityService.Infrastructure.Models;
using IdentityService.Infrastructure.Notifications;
using IdentityService.Services.Interfaces;
using IdentityService.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace IdentityService.Services;

public class AuthService(
    IUserRepository         userRepo,
    IRefreshTokenRepository refreshTokenRepo,
    IJwtTokenService        jwtService,
    IEmailService           emailService,
    IPasswordResetRepository resetRepo,
    INotificationClient     notificationClient,
    IOptions<JwtSettings>   jwtOptions,
    ILogger<AuthService>    logger
) : IAuthService
{
    private readonly JwtSettings _jwtSettings = jwtOptions.Value;

    // ─── REGISTER ──────────────────────────────────────────────────────────────
    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
    {
        // Verificările de unicitate (email, username, currency) sunt deja făcute
        // de RegisterRequestValidator (FluentValidation) înainte să ajungă aici.
        // Singura apărare rămasă sunt constrângerile UNIQUE din DB (gestionate de
        // GlobalExceptionMiddleware prin PostgresException 23505 → 409 Conflict).

        var normalizedEmail = request.Email.ToLowerInvariant().Trim();
        var confirmationToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        var user = new User
        {
            Email                  = normalizedEmail,
            Username               = request.Username.Trim(),
            PasswordHash           = PasswordHasher.Hash(request.Password),
            FirstName              = request.FirstName?.Trim(),
            LastName               = request.LastName?.Trim(),
            Status                 = UserStatus.Pending,
            PreferredCurrencyId    = request.PreferredCurrencyId,
            EmailConfirmationToken = confirmationToken,
        };

        var userId = await userRepo.CreateAsync(user);
        logger.LogInformation("User {UserId} registered with status PENDING", userId);

        await emailService.SendConfirmationEmailAsync(user.Email, confirmationToken);

        return new RegisterResponse(
            UserId:   userId,
            Email:    user.Email,
            Username: user.Username,
            Status:   UserStatus.Pending,
            Message:  "Cont creat. Verifica email-ul pentru activare."
        );
    }

    // ─── LOGIN ─────────────────────────────────────────────────────────────────
    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var normalizedEmail = request.Email.ToLowerInvariant().Trim();

        var user = await userRepo.GetByEmailAsync(normalizedEmail)
            ?? throw new UnauthorizedException("Credentiale invalide.");

        if (user.Status == UserStatus.Pending)
            throw new ForbiddenException("Contul nu este confirmat. Verifica email-ul.");

        if (user.Status == UserStatus.Inactive)
            throw new ForbiddenException("Contul este dezactivat.");

        if (!PasswordHasher.Verify(request.Password, user.PasswordHash))
        {
            await userRepo.IncrementFailedAttemptsAsync(user.Id);
            throw new UnauthorizedException("Credentiale invalide.");
        }

        await userRepo.UpdateLastLoginAsync(user.Id);

        var accessToken  = jwtService.GenerateAccessToken(user);
        var refreshToken = jwtService.GenerateRefreshToken();
        var familyId     = await refreshTokenRepo.NextFamilyIdAsync();
        await refreshTokenRepo.CreateAsync(user.Id, refreshToken, GetRefreshTokenExpiresAt(), familyId);

        logger.LogInformation("User {UserId} logged in successfully", user.Id);
        return BuildLoginResponse(accessToken, refreshToken, user);
    }

    // ─── CONFIRM EMAIL ──────────────────────────────────────────────────────────
    public async Task ConfirmEmailAsync(string token)
    {
        await userRepo.ConfirmEmailAsync(token);
    }

    // ─── REFRESH TOKEN ──────────────────────────────────────────────────────────
    public async Task<LoginResponse> RefreshTokenAsync(string refreshToken)
    {
        var storedToken = await refreshTokenRepo.GetByTokenAsync(refreshToken)
            ?? throw new UnauthorizedException("Refresh token invalid.");

        // Reuse-detection: un token DEJA REVOCAT re-prezentat = posibil furt.
        // Verificat INAINTEA expirarii ca sa prindem replay-ul indiferent de varsta.
        if (storedToken.IsRevoked)
        {
            await refreshTokenRepo.RevokeFamilyAsync(storedToken.FamilyId);
            logger.LogWarning(
                "Refresh token reuse detectat pentru user {UserId}; familia {FamilyId} a fost revocata.",
                storedToken.UserId, storedToken.FamilyId);
            throw new UnauthorizedException("Token reuse detectat. Toate sesiunile au fost revocate.");
        }

        if (storedToken.IsExpired)
            throw new UnauthorizedException("Refresh token expirat.");

        var user = await userRepo.GetByIdAsync(storedToken.UserId)
            ?? throw new UnauthorizedException("Utilizatorul nu mai exista.");

        if (user.Status == UserStatus.Inactive)
            throw new ForbiddenException("Contul este dezactivat.");

        // Rotatie: revoca tokenul curent, emite unul nou in ACEEASI familie.
        await refreshTokenRepo.RevokeAsync(refreshToken);

        var newAccessToken  = jwtService.GenerateAccessToken(user);
        var newRefreshToken = jwtService.GenerateRefreshToken();
        await refreshTokenRepo.CreateAsync(
            user.Id, newRefreshToken, GetRefreshTokenExpiresAt(), storedToken.FamilyId);

        logger.LogInformation("Refresh token rotated for user {UserId} (family {FamilyId})",
            user.Id, storedToken.FamilyId);
        return BuildLoginResponse(newAccessToken, newRefreshToken, user);
    }

    // ─── LOGOUT ─────────────────────────────────────────────────────────────────
    public async Task LogoutAsync(string refreshToken)
    {
        await refreshTokenRepo.RevokeAsync(refreshToken);
        logger.LogInformation("Refresh token revoked on logout");
    }

    // ─── FORGOT PASSWORD ────────────────────────────────────────────────────────
    public async Task ForgotPasswordAsync(string email, string frontendBaseUrl)
    {
        var normalized = email.ToLowerInvariant().Trim();
        var user = await userRepo.GetByEmailAsync(normalized);
        if (user is null)
        {
            // Anti user-enumeration: nu dezvaluim ca emailul nu exista.
            logger.LogInformation("Forgot-password pentru email inexistent (ignorat silentios).");
            return;
        }

        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var expiresAt = DateTime.UtcNow.AddHours(1);
        await resetRepo.CreateAsync(user.Id, token, expiresAt);

        var link = $"{frontendBaseUrl.TrimEnd('/')}/reset-password?token={Uri.EscapeDataString(token)}";
        await notificationClient.SendPasswordResetAsync(user.Email, user.FirstName, link);
        logger.LogInformation("Token de reset creat pentru user {UserId}", user.Id);
    }

    // ─── RESET PASSWORD ─────────────────────────────────────────────────────────
    public async Task ResetPasswordAsync(string token, string newPassword)
    {
        var active = await resetRepo.GetActiveAsync(token)
            ?? throw new ValidationException("Token invalid sau expirat.");

        await userRepo.ChangePasswordAsync(active.UserId, PasswordHasher.Hash(newPassword));
        await resetRepo.ConsumeAsync(token);
        await refreshTokenRepo.RevokeAllForUserAsync(active.UserId);
        logger.LogInformation("Parola resetata + sesiuni revocate pentru user {UserId}", active.UserId);
    }

    // ─── HELPERS ────────────────────────────────────────────────────────────────

    private LoginResponse BuildLoginResponse(string accessToken, string refreshToken, User user) =>
        new(
            AccessToken:  accessToken,
            RefreshToken: refreshToken,
            ExpiresIn:    _jwtSettings.ExpiryMinutes * 60,
            TokenType:    "Bearer",
            User:         MapToUserDto(user)
        );

    private DateTime GetRefreshTokenExpiresAt() =>
        DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays);

    private static UserDto MapToUserDto(User u) =>
        new(u.Id, u.Email, u.Username, u.FirstName, u.LastName, u.Status, u.PreferredCurrencyId);
}

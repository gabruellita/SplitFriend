using FluentValidation;
using IdentityService.API.Security;
using IdentityService.DTO.Requests;
using IdentityService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.API.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController(
    IAuthService                  authService,
    IProfileService               profileService,
    ICurrentUser                  currentUser,
    IValidator<RegisterRequest>      registerValidator,
    IValidator<LoginRequest>         loginValidator,
    IValidator<ResetPasswordRequest> resetValidator,
    IValidator<ForgotPasswordRequest> forgotValidator,
    IConfiguration                   config
) : ControllerBase
{
    /// <summary>Inregistrare utilizator nou. Contul va fi PENDING pana la confirmarea email-ului.</summary>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var validation = await registerValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));

        var result = await authService.RegisterAsync(request);
        return CreatedAtAction(nameof(Register), new { id = result.UserId }, result);
    }

    /// <summary>Autentificare. Returneaza JWT access token + refresh token.</summary>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var validation = await loginValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));

        var result = await authService.LoginAsync(request);
        return Ok(result);
    }

    /// <summary>Confirma email-ul cu tokenul primit. Activeaza contul (PENDING → ACTIVE).</summary>
    [HttpPost("confirm-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request)
    {
        await authService.ConfirmEmailAsync(request.Token);
        return Ok(new { message = "Email confirmat. Contul este acum ACTIV." });
    }

    /// <summary>Refresh token rotation: primeste token vechi, returneaza access + refresh token noi.</summary>
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var result = await authService.RefreshTokenAsync(request.RefreshToken);
        return Ok(result);
    }

    /// <summary>Delogare: revoca refresh token-ul curent. Celelalte sesiuni raman active.</summary>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        await authService.LogoutAsync(request.RefreshToken);
        return NoContent();
    }

    /// <summary>Cere reset de parola. Raspunde 200 mereu (nu dezvaluie daca emailul exista).</summary>
    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var validation = await forgotValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));

        var frontendBaseUrl = config["App:FrontendBaseUrl"] ?? "http://localhost:5173";
        await authService.ForgotPasswordAsync(request.Email, frontendBaseUrl);
        return Ok(new { message = "Daca exista un cont cu acest email, vei primi un link de resetare." });
    }

    /// <summary>Reseteaza parola cu tokenul primit pe email. Revoca toate sesiunile.</summary>
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var validation = await resetValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));

        await authService.ResetPasswordAsync(request.Token, request.NewPassword);
        return Ok(new { message = "Parola a fost resetata. Te poti autentifica acum." });
    }

    /// <summary>Profilul utilizatorului curent (citit din X-User-Id de la Gateway).</summary>
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me()
        => Ok(await profileService.GetMeAsync(currentUser.UserId));
}

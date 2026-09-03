using FluentValidation;
using IdentityService.API.Security;
using IdentityService.DTO.Requests;
using IdentityService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.API.Controllers;

[ApiController]
[Route("api/me")]
[Produces("application/json")]
public class MeController(
    IProfileService                     profileService,
    ICurrentUser                        currentUser,
    IValidator<UpdateProfileRequest>    updateValidator,
    IValidator<ChangePasswordRequest>   changePasswordValidator
) : ControllerBase
{
    /// <summary>Actualizeaza nume si/sau moneda preferata (patch partial).</summary>
    [HttpPatch]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update([FromBody] UpdateProfileRequest request)
    {
        var validation = await updateValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));

        return Ok(await profileService.UpdateProfileAsync(currentUser.UserId, request));
    }

    /// <summary>Schimba parola; revoca toate sesiunile.</summary>
    [HttpPost("change-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var validation = await changePasswordValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));

        await profileService.ChangePasswordAsync(currentUser.UserId, request);
        return NoContent();
    }
}

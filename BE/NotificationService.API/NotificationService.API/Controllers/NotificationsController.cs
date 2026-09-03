using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NotificationService.DTO;
using NotificationService.Services.Interfaces;

namespace NotificationService.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Produces("application/json")]
public class NotificationsController(
    INotificationService          service,
    IValidator<SendEmailRequest>  validator
) : ControllerBase
{
    /// <summary>Trimite un email pe baza unui template. Endpoint intern (apelat de alte servicii).</summary>
    [HttpPost("email")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendEmail([FromBody] SendEmailRequest request)
    {
        var validation = await validator.ValidateAsync(request);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));

        await service.SendEmailAsync(request);
        return Accepted();
    }
}

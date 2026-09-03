namespace IdentityService.Services.Interfaces;

public interface IEmailService
{
    Task SendConfirmationEmailAsync(string toEmail, string token);
}

using NotificationService.DTO;

namespace NotificationService.Services.Interfaces;

public interface INotificationService
{
    Task SendEmailAsync(SendEmailRequest request);
}

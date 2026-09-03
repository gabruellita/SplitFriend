namespace IdentityService.Infrastructure.Notifications;

public interface INotificationClient
{
    /// <summary>Trimite email-ul de reset parola. Nu arunca daca serviciul e jos (best-effort).</summary>
    Task SendPasswordResetAsync(string toEmail, string? firstName, string link);
}

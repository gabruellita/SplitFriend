namespace FinanceService.Infrastructure.Notifications;

public interface INotificationClient
{
    /// <summary>Trimite email-ul de invitatie in grup. Nu arunca daca serviciul e jos (best-effort).</summary>
    Task SendGroupInviteAsync(string toEmail, string groupName, string link);
}

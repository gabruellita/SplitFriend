using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace FinanceService.Infrastructure.Notifications;

public class NotificationClient(HttpClient http, ILogger<NotificationClient> logger) : INotificationClient
{
    public async Task SendGroupInviteAsync(string toEmail, string groupName, string link)
    {
        try
        {
            var payload = new
            {
                to       = toEmail,
                template = "group-invite",
                data     = new Dictionary<string, string> { ["groupName"] = groupName, ["link"] = link }
            };
            var resp = await http.PostAsJsonAsync("/api/notifications/email", payload);
            if (!resp.IsSuccessStatusCode)
                logger.LogWarning("Notification a raspuns {Status} pentru invitatia catre {Email}", resp.StatusCode, toEmail);
        }
        catch (Exception ex)
        {
            // Best-effort: invitatia exista deja in DB; email-ul e secundar (decizia #10).
            logger.LogWarning(ex, "Nu am putut trimite email-ul de invitatie catre {Email}", toEmail);
        }
    }
}

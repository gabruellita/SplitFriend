using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace IdentityService.Infrastructure.Notifications;

public class NotificationClient(HttpClient http, ILogger<NotificationClient> logger) : INotificationClient
{
    public async Task SendPasswordResetAsync(string toEmail, string? firstName, string link)
    {
        try
        {
            var payload = new
            {
                to       = toEmail,
                template = "password-reset",
                data     = new Dictionary<string, string>
                {
                    ["firstName"] = firstName ?? "",
                    ["link"]      = link
                }
            };
            var resp = await http.PostAsJsonAsync("/api/notifications/email", payload);
            if (!resp.IsSuccessStatusCode)
                logger.LogWarning("Notification a raspuns {Status} pentru reset catre {Email}", resp.StatusCode, toEmail);
        }
        catch (Exception ex)
        {
            // Best-effort: controllerul raspunde 200 oricum (anti user-enumeration).
            logger.LogWarning(ex, "Nu am putut trimite email-ul de reset catre {Email}", toEmail);
        }
    }
}

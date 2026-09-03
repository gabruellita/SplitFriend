using System.Net;
using NotificationService.DTO;
using NotificationService.Infrastructure.Email;
using NotificationService.Infrastructure.Exceptions;
using NotificationService.Services.Interfaces;

namespace NotificationService.Services;

public class NotificationOrchestrator(IEmailSender emailSender) : INotificationService
{
    public async Task SendEmailAsync(SendEmailRequest request)
    {
        var (subject, body) = BuildContent(request.Template, request.Data);
        await emailSender.SendAsync(request.To, subject, body);
    }

    private static (string subject, string body) BuildContent(
        string template, IReadOnlyDictionary<string, string> data)
    {
        switch (template)
        {
            case "group-invite":
            {
                var groupName = data.GetValueOrDefault("groupName", "un grup");
                var link      = data.GetValueOrDefault("link", "http://localhost:5173");
                var subject   = $"Ai fost invitat in grupul \"{groupName}\" — FinanceApp";
                var body = $"""
                    <h2>Invitatie in grup</h2>
                    <p>Ai fost invitat sa te alaturi grupului <strong>{groupName}</strong> in FinanceApp.</p>
                    <p><a href="{link}">Deschide invitatia</a></p>
                    """;
                return (subject, body);
            }
            case "password-reset":
            {
                var firstName = WebUtility.HtmlEncode(data.GetValueOrDefault("firstName", ""));
                var link      = data.GetValueOrDefault("link", "http://localhost:5173");
                var subject   = "Resetare parola — FinanceApp";
                var body = $"""
                    <p>Salut {firstName},</p>
                    <p>Ai cerut resetarea parolei. Apasa linkul de mai jos (valabil 1 ora):</p>
                    <p><a href="{link}">Reseteaza parola</a></p>
                    <p>Daca nu ai cerut tu acest reset, ignora acest email.</p>
                    """;
                return (subject, body);
            }
            default:
                throw new NotificationException($"Template necunoscut: '{template}'.");
        }
    }
}

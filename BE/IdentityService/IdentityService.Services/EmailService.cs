using IdentityService.Services.Interfaces;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace IdentityService.Services;

public class EmailService(IConfiguration config, ILogger<EmailService> logger) : IEmailService
{
    public async Task SendConfirmationEmailAsync(string toEmail, string token)
    {
        var smtpHost  = config["Smtp:Host"]      ?? "localhost";
        var smtpPort  = int.Parse(config["Smtp:Port"] ?? "1025");
        var fromEmail = config["Smtp:FromEmail"] ?? "noreply@financeapp.local";
        var fromName  = config["Smtp:FromName"]  ?? "FinanceApp";

        var confirmLink = $"http://localhost:5173/confirm-email?token={Uri.EscapeDataString(token)}";

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromEmail));
        message.To.Add(new MailboxAddress(toEmail, toEmail));
        message.Subject = "Confirma-ti contul — FinanceApp";

        message.Body = new TextPart("html")
        {
            Text = $"""
                <h2>Bun venit la FinanceApp!</h2>
                <p>Click pe link-ul de mai jos pentru a-ti activa contul:</p>
                <a href="{confirmLink}">{confirmLink}</a>
                <p>Link-ul este valabil 24 de ore.</p>
                """
        };

        using var client = new SmtpClient();
        await client.ConnectAsync(smtpHost, smtpPort, MailKit.Security.SecureSocketOptions.None);
        await client.SendAsync(message);
        await client.DisconnectAsync(quit: true);

        logger.LogInformation("Confirmation email sent to {Email} via MailHog", toEmail);
    }
}

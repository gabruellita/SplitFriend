using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace NotificationService.Infrastructure.Email;

public class SmtpEmailSender(IConfiguration config, ILogger<SmtpEmailSender> logger) : IEmailSender
{
    public async Task SendAsync(string toEmail, string subject, string htmlBody)
    {
        var smtpHost  = config["Smtp:Host"]      ?? "localhost";
        var smtpPort  = int.Parse(config["Smtp:Port"] ?? "1025");
        var fromEmail = config["Smtp:FromEmail"] ?? "noreply@financeapp.local";
        var fromName  = config["Smtp:FromName"]  ?? "FinanceApp";

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromEmail));
        message.To.Add(new MailboxAddress(toEmail, toEmail));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };

        using var client = new SmtpClient();
        await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.None);
        await client.SendAsync(message);
        await client.DisconnectAsync(quit: true);

        logger.LogInformation("Email '{Subject}' trimis catre {Email} via MailHog", subject, toEmail);
    }
}

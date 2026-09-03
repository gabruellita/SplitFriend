using NotificationService.DTO;
using NotificationService.Infrastructure.Email;
using NotificationService.Infrastructure.Exceptions;
using NotificationService.Services;

namespace NotificationService.Tests;

/// <summary>
/// Teste pentru constructia continutului de e-mail (NotificationOrchestrator, sectiunea 4.5.5):
/// alegerea sablonului, completarea datelor, escaparea HTML si respingerea sabloanelor necunoscute.
/// Trimiterea SMTP este inlocuita cu un fake care captureaza mesajul.
/// </summary>
public class NotificationOrchestratorTests
{
    private sealed class CapturingEmailSender : IEmailSender
    {
        public string? To { get; private set; }
        public string? Subject { get; private set; }
        public string? Body { get; private set; }
        public int SendCount { get; private set; }

        public Task SendAsync(string toEmail, string subject, string htmlBody)
        {
            To = toEmail; Subject = subject; Body = htmlBody; SendCount++;
            return Task.CompletedTask;
        }
    }

    private static SendEmailRequest Req(string template, Dictionary<string, string> data, string to = "user@test.ro")
        => new(to, template, data);

    [Fact]
    public async Task GroupInvite_ConstruiesteSubiectSiCorpCuNumeleGrupuluiSiLink()
    {
        var sender = new CapturingEmailSender();
        var orch = new NotificationOrchestrator(sender);

        await orch.SendEmailAsync(Req("group-invite", new()
        {
            ["groupName"] = "Vacanta Grecia",
            ["link"] = "http://localhost:5173/app/groups/7?invite=1"
        }));

        Assert.Equal(1, sender.SendCount);
        Assert.Equal("user@test.ro", sender.To);
        Assert.Contains("Vacanta Grecia", sender.Subject);
        Assert.Contains("Vacanta Grecia", sender.Body);
        Assert.Contains("http://localhost:5173/app/groups/7?invite=1", sender.Body);
    }

    [Fact]
    public async Task PasswordReset_EscapeazaNumeleInHtml()
    {
        var sender = new CapturingEmailSender();
        var orch = new NotificationOrchestrator(sender);

        await orch.SendEmailAsync(Req("password-reset", new()
        {
            ["firstName"] = "<script>alert(1)</script>",
            ["link"] = "http://localhost:5173/reset?token=abc"
        }));

        Assert.Contains("Resetare parola", sender.Subject);
        Assert.DoesNotContain("<script>", sender.Body);          // HtmlEncode aplicat
        Assert.Contains("&lt;script&gt;", sender.Body);
    }

    [Fact]
    public async Task TemplateNecunoscut_AruncaNotificationException_FaraTrimitere()
    {
        var sender = new CapturingEmailSender();
        var orch = new NotificationOrchestrator(sender);

        await Assert.ThrowsAsync<NotificationException>(() =>
            orch.SendEmailAsync(Req("inexistent", new())));

        Assert.Equal(0, sender.SendCount); // nu s-a trimis nimic
    }

    [Fact]
    public async Task GroupInvite_FaraDate_FolosesteValoriImplicite()
    {
        var sender = new CapturingEmailSender();
        var orch = new NotificationOrchestrator(sender);

        await orch.SendEmailAsync(Req("group-invite", new()));

        Assert.Equal(1, sender.SendCount);
        Assert.Contains("un grup", sender.Subject); // default groupName
    }
}

using AutoLuki.Application.Common;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace AutoLuki.Infrastructure.Email;

public class EmailOptions
{
    public const string SectionName = "Email";

    public string? SmtpHost { get; init; }
    public int SmtpPort { get; init; } = 587;
    public string? Username { get; init; }
    public string? Password { get; init; }
    public string FromAddress { get; init; } = "no-reply@autoluki.local";
    public string FromName { get; init; } = "Auto Sallon Luki";

    public string? NotificationAddress { get; init; }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(SmtpHost);
}

public class SmtpEmailSender(IOptions<EmailOptions> options) : IEmailSender
{
    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        var o = options.Value;
        var mime = new MimeMessage();
        mime.From.Add(new MailboxAddress(o.FromName, o.FromAddress));
        mime.To.Add(MailboxAddress.Parse(message.To));
        mime.Subject = message.Subject;
        mime.Body = new TextPart("plain") { Text = message.TextBody };

        var host = o.SmtpHost ?? throw new InvalidOperationException("Email:SmtpHost is not configured.");

        using var client = new SmtpClient();
        await client.ConnectAsync(host, o.SmtpPort, SecureSocketOptions.StartTlsWhenAvailable, cancellationToken);
        if (!string.IsNullOrEmpty(o.Username))
        {
            await client.AuthenticateAsync(o.Username, o.Password ?? "", cancellationToken);
        }

        await client.SendAsync(mime, cancellationToken);
        await client.DisconnectAsync(quit: true, cancellationToken);
    }
}

public class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IEmailSender
{
    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Email (not sent, SMTP not configured) to {To}: {Subject}\n{Body}",
            message.To, message.Subject, message.TextBody);
        return Task.CompletedTask;
    }
}

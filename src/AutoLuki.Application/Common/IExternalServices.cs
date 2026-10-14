namespace AutoLuki.Application.Common;

public interface ICaptchaVerifier
{
    Task<bool> VerifyAsync(string token, string? remoteIp, CancellationToken cancellationToken = default);
}

public record EmailMessage(string To, string Subject, string TextBody);

public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}

public interface IInquiryNotificationQueue
{
    ValueTask EnqueueAsync(int inquiryId, CancellationToken cancellationToken = default);
}

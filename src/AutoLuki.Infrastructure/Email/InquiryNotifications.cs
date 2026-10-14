using System.Text;
using System.Threading.Channels;
using AutoLuki.Application.Common;
using AutoLuki.Application.Inquiries;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AutoLuki.Infrastructure.Email;

public class InquiryNotificationQueue : IInquiryNotificationQueue
{
    private readonly Channel<int> _channel = Channel.CreateBounded<int>(new BoundedChannelOptions(500)
    {
        FullMode = BoundedChannelFullMode.DropOldest,
        SingleReader = true,
    });

    public ChannelReader<int> Reader => _channel.Reader;

    public ValueTask EnqueueAsync(int inquiryId, CancellationToken cancellationToken = default) =>
        _channel.Writer.WriteAsync(inquiryId, cancellationToken);
}

public class InquiryNotificationWorker(
    InquiryNotificationQueue queue,
    IServiceScopeFactory scopeFactory,
    IOptions<EmailOptions> emailOptions,
    ILogger<InquiryNotificationWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var inquiryId in queue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await NotifyAsync(inquiryId, stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Failed to send notification for inquiry {InquiryId}", inquiryId);
            }
        }
    }

    private async Task NotifyAsync(int inquiryId, CancellationToken cancellationToken)
    {
        var to = emailOptions.Value.NotificationAddress;
        if (string.IsNullOrWhiteSpace(to))
        {
            logger.LogWarning("Email:NotificationAddress is not set; skipping notification for inquiry {InquiryId}", inquiryId);
            return;
        }

        await using var scope = scopeFactory.CreateAsyncScope();
        var inquiry = await scope.ServiceProvider.GetRequiredService<IInquiryService>().GetAsync(inquiryId, cancellationToken);
        var sender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

        var body = new StringBuilder()
            .AppendLine($"Type: {inquiry.Type}")
            .AppendLine($"Car: {inquiry.Car?.Title ?? "(general inquiry)"}")
            .AppendLine($"Name: {inquiry.Name}")
            .AppendLine($"Phone: {inquiry.Phone}")
            .AppendLine($"Email: {inquiry.Email ?? "-"}")
            .AppendLine($"Preferred date: {inquiry.PreferredDate?.ToString("dd.MM.yyyy") ?? "-"}")
            .AppendLine()
            .AppendLine(inquiry.Message)
            .ToString();

        var subject = $"New inquiry from {inquiry.Name}" + (inquiry.Car is null ? "" : $" - {inquiry.Car.Title}");
        await sender.SendAsync(new EmailMessage(to, subject, body), cancellationToken);
        logger.LogInformation("Notification sent for inquiry {InquiryId}", inquiryId);
    }
}

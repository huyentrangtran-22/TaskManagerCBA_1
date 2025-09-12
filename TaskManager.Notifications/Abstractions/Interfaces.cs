using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.Notifications.Abstractions
{
    public interface INotificationPublisher
    {
        Task PublishAsync(NotificationEvent ev, CancellationToken ct = default);
    }

    public interface INotificationService
    {
        Task EnqueueAsync(NotificationEvent ev, CancellationToken ct = default);
        Task<int> ProcessPendingAsync(CancellationToken ct = default);
    }

    public record EmailMessage(string To, string Subject, string HtmlBody, string? TextBody = null);

    public record EmailSendResult(bool Success, string? ProviderMessageId = null, string? Error = null)
    {
        public static EmailSendResult Sent(string? id = null) => new(true, id, null);
        public static EmailSendResult Failed(string error) => new(false, null, error);
    }

    public interface IEmailSender
    {
        Task<EmailSendResult> SendAsync(EmailMessage message, CancellationToken ct = default);
    }

    public interface ITemplateRenderer
    {
        Task<(string Subject, string HtmlBody, string? TextBody)> RenderAsync(
            string templateKey, string locale, object model, CancellationToken ct = default);
    }

    public interface INotificationScheduler
    {
        Task<int> ScheduleDueDateRemindersAsync(DateTime utcNow, CancellationToken ct = default);
    }
}

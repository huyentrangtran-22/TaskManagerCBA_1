using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.Notifications.Abstractions
{
    public record NotificationEvent(
        Guid EventId,
        NotificationEventType Type,
        DateTime OccurredAtUtc,
        string ActorUserId,
        string TargetUserId,
        string? BoardId = null,
        string? CardId = null,
        string? CommentId = null,
        string? Description = null,
        DateTime? DueAtUtc = null,
        IDictionary<string, string>? Extra = null
        );
}

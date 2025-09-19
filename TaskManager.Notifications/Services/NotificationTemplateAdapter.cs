using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskManager.Notifications.Abstractions;             // event bus (EnqueueAsync)
using TaskManager.Notifications.Abstractions;
using TaskManager.Notifications.Services;     // IUserNotificationService (service của app)
using TaskManager.Shared.Entities;            // entity Notification (nếu bạn đang dùng)

namespace TaskManager.Notifications.Services
{
    /// <summary>
    /// Adapter: từ lời gọi SendNotificationAsync của app -> đẩy sự kiện vào Outbox để gửi email theo template.
    /// Không tham chiếu sang ProjectDbContext / AppDbContext để tránh phụ thuộc vòng.
    /// </summary>
    public sealed class NotificationTemplateAdapter : IUserNotificationService
    {
        private readonly Notifications.Abstractions.INotificationService _eventBus;

        public NotificationTemplateAdapter(Notifications.Abstractions.INotificationService eventBus)
        {
            _eventBus = eventBus;
        }

        public Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId)
            => Task.FromResult<IEnumerable<Notification>>(Array.Empty<Notification>());

        public async Task<Notification> SendNotificationAsync(string userId, string message)
        {
            // Format: ADDED_TO_PROJECT|{projectId}|{projectName}|{actorUserId}|{actorName}|{targetName}
            if (TryParseAddedToProject(message,
                    out var projectId, out var projectName,
                    out var actorUserId, out var actorName, out var targetName))
            {
                await _eventBus.EnqueueAsync(new NotificationEvent(
                    EventId: Guid.NewGuid(),
                    Type: NotificationEventType.AddedAsMemberToBoard,
                    OccurredAtUtc: DateTime.UtcNow,
                    ActorUserId: actorUserId!,
                    TargetUserId: userId,
                    BoardId: projectId!.Value.ToString(),
                    Description: $"User {targetName} được thêm vào {projectName}",
                    Extra: new Dictionary<string, string>
                    {
                        ["project_name"] = projectName!,
                        ["project_url"] = $"/projects/{projectId}",
                        ["actor_name"] = actorName!,
                        ["target_name"] = targetName!
                    }
                ));
            }

            return new Notification
            {
                Id = 0,
                UserId = userId,
                Message = message,
                CreatedAt = DateTime.UtcNow
            };
        }

        public Task<bool> MarkAsReadAsync(int notificationId) => Task.FromResult(true);
        public Task CheckOverdueTasksAsync() => Task.CompletedTask;
        public Task<bool> DeleteAsync(int id) => Task.FromResult(true);

        private static bool TryParseAddedToProject(
            string input,
            out int? projectId,
            out string? projectName,
            out string? actorUserId,
            out string? actorName,
            out string? targetName)
        {
            projectId = null; projectName = actorUserId = actorName = targetName = null;

            if (string.IsNullOrWhiteSpace(input)) return false;
            var parts = input.Split('|', StringSplitOptions.TrimEntries);
            if (parts.Length != 6) return false;
            if (!parts[0].Equals("ADDED_TO_PROJECT", StringComparison.OrdinalIgnoreCase)) return false;
            if (!int.TryParse(parts[1], out var pid)) return false;

            projectId = pid;
            projectName = parts[2];
            actorUserId = parts[3];
            actorName = parts[4];
            targetName = parts[5];
            return true;
        }
    }
}

using Microsoft.EntityFrameworkCore;
using TaskManager.Notifications.Abstractions;                   // ✅ đúng namespace event-bus
using TaskManager.Notifications.Abstractions;
using TaskManager.Projects.Data;                   // ProjectDbContext (Projects, ProjectMembers)
using TaskManager.Shared.Data;                     // AppDbContext (AspNetUsers)
// alias entity nếu ProjectMembers dùng entity ở Shared
using ProjectMemberEntity = TaskManager.Shared.Entities.ProjectMember;

namespace TaskManager.Projects.Services;

public sealed class ProjectMembershipService
{
    private readonly ProjectDbContext _projectsDb;
    private readonly AppDbContext _identityDb;
    private readonly INotificationService _notifier;   // từ Notifications.Abstractions

    public ProjectMembershipService(
        ProjectDbContext projectsDb,
        AppDbContext identityDb,
        INotificationService notifier)
    {
        _projectsDb = projectsDb;
        _identityDb = identityDb;
        _notifier = notifier;
    }

    public async Task AddMemberAndNotifyAsync(
        int projectId, string addedUserId, string actorUserId, CancellationToken ct = default)
    {
        // 1) Idempotent: đã có thì không thêm lại
        var exists = await _projectsDb.ProjectMembers
            .AnyAsync(x => x.ProjectId == projectId && x.UserId == addedUserId, ct);

        if (!exists)
        {
            _projectsDb.ProjectMembers.Add(new ProjectMemberEntity
            {
                ProjectId = projectId,
                UserId = addedUserId
            });

            // ✅ nhớ lưu thay đổi
            await _projectsDb.SaveChangesAsync(ct);
        }

        // 2) Lấy thông tin hiển thị (phòng null)
        var projectName = await _projectsDb.Projects
            .Where(p => p.Id == projectId)
            .Select(p => p.Name)
            .FirstOrDefaultAsync(ct) ?? $"Project #{projectId}";

        var actorName = await _identityDb.Users
            .Where(u => u.Id == actorUserId)
            .Select(u => (u.FullName ?? u.UserName)!)
            .FirstOrDefaultAsync(ct) ?? "Hệ thống";

        var targetName = await _identityDb.Users
            .Where(u => u.Id == addedUserId)
            .Select(u => (u.FullName ?? u.UserName)!)
            .FirstOrDefaultAsync(ct) ?? "Người dùng";

        // 3) Đẩy sự kiện AddedAsMemberToBoard vào Outbox
        await _notifier.EnqueueAsync(new NotificationEvent(
            EventId: Guid.NewGuid(),
            Type: NotificationEventType.AddedAsMemberToBoard,
            OccurredAtUtc: DateTime.UtcNow,
            ActorUserId: actorUserId,
            TargetUserId: addedUserId,
            BoardId: projectId.ToString(),
            Description: $"User {targetName} được thêm vào {projectName}",
            Extra: new Dictionary<string, string>
            {
                ["project_name"] = projectName,
                ["project_url"] = $"/projects/{projectId}",
                ["actor_name"] = actorName,
                ["target_name"] = targetName
            }
        ), ct);
    }
}

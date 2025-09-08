using TaskManager.Shared.Entities;

namespace TaskManager.Notifications.Services
{
    public interface INotificationService
    {
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId);
        Task<Notification> SendNotificationAsync(string userId, string message);
        Task<bool> MarkAsReadAsync(int notificationId);
        Task CheckOverdueTasksAsync(); // kiểm tra task chậm tiến độ
        Task<bool> DeleteAsync(int id);
    }
}

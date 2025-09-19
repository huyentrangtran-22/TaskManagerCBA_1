using System.Collections.Generic;
using System.Threading.Tasks;
using TaskManager.Shared.Entities; 

namespace TaskManager.Notifications.Services
{
    public interface IUserNotificationService
    {
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId);
        Task<Notification> SendNotificationAsync(string userId, string message);
        Task<bool> MarkAsReadAsync(int notificationId);
        Task CheckOverdueTasksAsync();
        Task<bool> DeleteAsync(int id);
    }
}

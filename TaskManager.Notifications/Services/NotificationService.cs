using Microsoft.EntityFrameworkCore;
using TaskManager.Shared.Data;
using TaskManager.Shared.Entities;

namespace TaskManager.Notifications.Services
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _context;

        public NotificationService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy tất cả notification của user
        /// </summary>
        public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Gửi notification tới user
        /// </summary>
        public async Task<Notification> SendNotificationAsync(string userId, string message)
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return notification;
        }

        /// <summary>
        /// Đánh dấu notification đã đọc
        /// </summary>
        public async Task<bool> MarkAsReadAsync(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification == null) return false;

            notification.IsRead = true;
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Xóa notification theo Id
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null) return false;

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Kiểm tra task quá hạn => gửi cảnh báo cho AssignedUser
        /// </summary>
        public async Task CheckOverdueTasksAsync()
        {
            var overdueTasks = await _context.TaskItems
                .Where(t => t.DueDate < DateTime.UtcNow && !t.IsCompleted)
                .ToListAsync();

            foreach (var task in overdueTasks)
            {
                if (!string.IsNullOrEmpty(task.AssignedUserId))
                {
                    await SendNotificationAsync(
                        task.AssignedUserId,
                        $"⚠️ Task '{task.Title}' đã quá hạn deadline ({task.DueDate})."
                    );
                }
            }
        }
    }
}

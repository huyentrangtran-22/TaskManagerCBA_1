using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Notifications.Services;

namespace TaskManager.Web.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // GET: /Notification
        public async Task<IActionResult> Index()
        {
            // Lấy userId hiện tại từ Identity
            string userId = User?.Identity?.Name ?? string.Empty;
            var notifications = await _notificationService.GetUserNotificationsAsync(userId);

            return View(notifications.OrderByDescending(n => n.CreatedAt));
        }

        // POST: /Notification/MarkAsRead/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var success = await _notificationService.MarkAsReadAsync(id);
            if (!success)
            {
                TempData["Error"] = "Cannot mark notification as read.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Notification/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _notificationService.DeleteAsync(id);
            if (!success)
            {
                TempData["Error"] = "Cannot delete notification.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

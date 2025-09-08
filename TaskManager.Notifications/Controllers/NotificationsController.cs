using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;
using TaskManager.Notifications.Services;
using TaskManager.Shared.Entities;

namespace TaskManager.Notifications.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserNotifications(string userId)
        {
            var notifications = await _notificationService.GetUserNotificationsAsync(userId);
            return Ok(notifications);
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendNotification([FromBody] Notification request)
        {
            var notification = await _notificationService.SendNotificationAsync(request.UserId, request.Message);
            return Ok(notification);
        }

        [HttpPut("read/{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var success = await _notificationService.MarkAsReadAsync(id);
            if (!success) return NotFound();
            return Ok();
        }

        [HttpPost("check-overdue")]
        public async Task<IActionResult> CheckOverdueTasks()
        {
            await _notificationService.CheckOverdueTasksAsync();
            return Ok("Đã kiểm tra và gửi cảnh báo task quá hạn.");
        }
    }
}

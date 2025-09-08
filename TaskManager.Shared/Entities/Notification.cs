using System.ComponentModel.DataAnnotations;

namespace TaskManager.Shared.Entities
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        // User nhận thông báo
        [Required, MaxLength(450)]
        public string UserId { get; set; } = null!;

        [Required]
        public string Message { get; set; } = null!;

        // Có thể liên kết tới task hoặc project (tuỳ dùng)
        public Guid? TaskId { get; set; }
        public Guid? ProjectId { get; set; }

        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

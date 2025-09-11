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

        //Có thể liên kết tới task hoặc project(tuỳ dùng)
        public int TaskId { get; set; }
        public int ProjectId { get; set; }
        public TaskItem TaskItem { get; set; } = null!;

        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

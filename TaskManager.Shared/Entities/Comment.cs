using System.ComponentModel.DataAnnotations;

namespace TaskManager.Shared.Entities
{
    public class Comment
    {
        [Key]
        public int Id { get; set; } 

        [Required]
        public int TaskId { get; set; }

        // Author user id (Identity)
        [Required, MaxLength(450)]
        public string AuthorUserId { get; set; } = null!;

        [Required]
        public string Content { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public TaskItem? TaskItem { get; set; }
    }
}

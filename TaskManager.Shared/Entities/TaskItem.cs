using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TaskManager.Shared.Entities;

namespace TaskManager.Shared.Entities
{
    public class TaskItem
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(300)]
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        [Required]
        public int ProjectId { get; set; }

        // Assigned user (Identity user id)
        [MaxLength(450)]
        public string? AssignedUserId { get; set; }

        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime EndDate { get; set; } = DateTime.UtcNow.AddDays(7);

        public DateTime? DueDate { get; set; }

        public TaskStatus Status { get; set; } 

        public bool IsCompleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Project Project { get; set; } = null!;
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}

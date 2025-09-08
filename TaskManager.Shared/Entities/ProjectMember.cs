using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManager.Shared.Entities
{
    public class ProjectMember
    {
        [Key]
        public int Id { get; set; } 

        [Required]
        public int ProjectId { get; set; }

        // UserId là Id của user trong Identity (string nếu dùng ASP.NET Identity)
        [Required, MaxLength(450)]
        public string UserId { get; set; } = null!;

        // Navigation
        public Project? Project { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TaskManager.Identity.Entities;
using TaskManager.Shared.Entities;

namespace TaskManager.Projects.Entities
{
    public class ProjectMember
    {
        internal readonly object UserName;

        [Key]
        public int Id { get; set; }

        // Liên kết với Project
        [Required]
        [ForeignKey(nameof(Project))]
        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;

        // Liên kết với User (Identity)
        [Required]
        [MaxLength(450)]
        [ForeignKey(nameof(User))]
        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
    }
}
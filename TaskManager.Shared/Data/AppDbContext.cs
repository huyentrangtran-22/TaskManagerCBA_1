using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskManager.Identity.Entities;
using TaskManager.Shared.Entities;

namespace TaskManager.Shared.Data
{
    // DbContext này sẽ có luôn bảng Identity (AspNetUsers, AspNetRoles...)
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Project> Projects { get; set; } = null!;
        public DbSet<ProjectMember> ProjectMembers { get; set; } = null!;
        public DbSet<TaskItem> TaskItems { get; set; } = null!;
        public DbSet<Comment> Comments { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Project - ProjectMember one-to-many
            modelBuilder.Entity<ProjectMember>()
                .HasOne(pm => pm.Project)
                .WithMany(p => p.Members)
                .HasForeignKey(pm => pm.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ Project - TaskItem one-to-many (cấu hình rõ ràng, không lặp)
            modelBuilder.Entity<TaskItem>()
                .HasOne(t => t.Project)
                .WithMany(p => p.Tasks)
                .HasForeignKey(t => t.ProjectId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // TaskItem - Comment one-to-many
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.TaskItem)
                .WithMany(t => t.Comments)
                .HasForeignKey(c => c.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes for performance
            modelBuilder.Entity<TaskItem>().HasIndex(t => t.ProjectId);
            modelBuilder.Entity<TaskItem>().HasIndex(t => t.AssignedUserId);
            modelBuilder.Entity<ProjectMember>().HasIndex(pm => new { pm.ProjectId, pm.UserId });
            modelBuilder.Entity<Notification>().HasIndex(n => n.UserId);
        }
    }
}
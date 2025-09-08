using Microsoft.EntityFrameworkCore;
using TaskManager.Shared.Entities;

namespace TaskManager.Shared.Data
{
    public static class DataSeeder
    {
        public static async Task SeedSampleDataAsync(AppDbContext db)
        {
            if (await db.Projects.AnyAsync()) return;

            var p1 = new Project { Name = "Project A", Description = "Dự án mẫu A" };
            var p2 = new Project { Name = "Project B", Description = "Dự án mẫu B" };

            var t1 = new TaskItem { Title = "Task 1", Description = "Mô tả 1", Project = p1, DueDate = DateTime.UtcNow.AddDays(7) };
            var t2 = new TaskItem { Title = "Task 2", Description = "Mô tả 2", Project = p1, DueDate = DateTime.UtcNow.AddDays(3) };

            db.Projects.AddRange(p1, p2);
            db.TaskItems.AddRange(t1, t2);

            await db.SaveChangesAsync();
        }
    }
}

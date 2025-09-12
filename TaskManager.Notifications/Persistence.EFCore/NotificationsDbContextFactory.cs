using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TaskManager.Notifications.Persistence.EFCore
{
    public class NotificationsDbContextFactory : IDesignTimeDbContextFactory<NotificationsDbContext>
    {
        public NotificationsDbContext CreateDbContext(string[] args)
        {
            // Lấy chuỗi kết nối từ ENV hoặc fallback cứng (đổi cho đúng máy bạn)
            var cs = Environment.GetEnvironmentVariable("NOTI_CS")
                     ?? "Server=(localdb)\\mssqllocaldb;Database=TaskManagerDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";

            var opts = new DbContextOptionsBuilder<NotificationsDbContext>()
                .UseSqlServer(cs)
                .Options;

            return new NotificationsDbContext(opts);
        }
    }
}

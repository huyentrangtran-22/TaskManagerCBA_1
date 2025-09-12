using Microsoft.EntityFrameworkCore;
using TaskManager.Notifications.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.Notifications.Persistence.EFCore;
public class NotificationUser
{
    public string UserId { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string? DisplayName { get; set; }
    public string Locale { get; set; } = "vi-VN";
}

public class NotificationPreference
{
    public string UserId { get; set; } = default!;
    public NotificationEventType EventType { get; set; }
    public bool EmailEnabled { get; set; } = true;
    public bool DigestEnabled { get; set; } = false;
    public string? ExtraJson { get; set; }
}

public class NotificationSubscription
{
    public string UserId { get; set; } = default!;
    public string Scope { get; set; } = default!; // "Board" | "Card"
    public string ScopeId { get; set; } = default!;
    public bool WatchAllChanges { get; set; } = true;
}

public class NotificationTemplate
{
    public string TemplateKey { get; set; } = default!;
    public string Locale { get; set; } = "vi-VN";
    public string SubjectTemplate { get; set; } = default!;
    public string HtmlTemplate { get; set; } = default!;
    public string? TextTemplate { get; set; }
}

public class NotificationOutbox
{
    public Guid OutboxId { get; set; }
    public NotificationEventType EventType { get; set; }
    public string PayloadJson { get; set; } = default!;
    public DateTime OccurredAtUtc { get; set; }
    public DateTime? ProcessedAtUtc { get; set; }
    public int RetryCount { get; set; }
    public DateTime NextAttemptUtc { get; set; }
}

public class NotificationDelivery
{
    public Guid DeliveryId { get; set; }
    public string UserId { get; set; } = default!;
    public NotificationEventType EventType { get; set; }
    public string Subject { get; set; } = default!;
    public string? BodyPreview { get; set; }
    public string Status { get; set; } = "Sent"; // Sent/Failed/Skipped
    public string? ProviderMessageId { get; set; }
    public string? Error { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? SentAtUtc { get; set; }
}

public class NotificationsDbContext : DbContext
{
    public NotificationsDbContext(DbContextOptions<NotificationsDbContext> options) : base(options) { }

    public DbSet<NotificationUser> Users => Set<NotificationUser>();
    public DbSet<NotificationPreference> Preferences => Set<NotificationPreference>();
    public DbSet<NotificationSubscription> Subscriptions => Set<NotificationSubscription>();
    public DbSet<NotificationTemplate> Templates => Set<NotificationTemplate>();
    public DbSet<NotificationOutbox> Outbox => Set<NotificationOutbox>();
    public DbSet<NotificationDelivery> Deliveries => Set<NotificationDelivery>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NotificationUser>().HasKey(x => x.UserId);

        modelBuilder.Entity<NotificationPreference>().HasKey(x => new { x.UserId, x.EventType });

        modelBuilder.Entity<NotificationSubscription>().HasKey(x => new { x.UserId, x.Scope, x.ScopeId });

        modelBuilder.Entity<NotificationTemplate>().HasKey(x => new { x.TemplateKey, x.Locale });

        modelBuilder.Entity<NotificationOutbox>().HasKey(x => x.OutboxId);
        modelBuilder.Entity<NotificationOutbox>().Property(x => x.PayloadJson).HasColumnType("nvarchar(max)");

        modelBuilder.Entity<NotificationDelivery>().HasKey(x => x.DeliveryId);
        modelBuilder.Entity<NotificationDelivery>().Property(x => x.Subject).HasMaxLength(512);
        modelBuilder.Entity<NotificationDelivery>().Property(x => x.BodyPreview).HasMaxLength(512);
        modelBuilder.Entity<NotificationDelivery>().Property(x => x.Status).HasMaxLength(16);
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // fallback an toàn (chỉ khi EF không bơm options):
            optionsBuilder.UseSqlServer("Server=.;Database=TaskManagerDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true");
        }
    }

}
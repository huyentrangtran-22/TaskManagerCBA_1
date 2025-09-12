using Microsoft.EntityFrameworkCore;                 // ✅ cho AsNoTracking/ToListAsync/CountAsync
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TaskManager.Notifications.Abstractions;
using TaskManager.Notifications.Persistence.EFCore;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;



namespace TaskManager.Notifications.Core
{
    public sealed class NotificationsOptions
    {
        public string BaseUrl { get; set; } = "https://localhost";
        public string FromEmail { get; set; } = "no-reply@example.com";
        public string Provider { get; set; } = "smtp";
        public int MaxSendPerBatch { get; set; } = 25;
    }

    public sealed class NotificationService : INotificationService, INotificationPublisher
    {
        private readonly NotificationsDbContext _db;
        public NotificationService(NotificationsDbContext db) => _db = db;

        public Task PublishAsync(NotificationEvent ev, CancellationToken ct = default) => EnqueueAsync(ev, ct);

        public async Task EnqueueAsync(NotificationEvent ev, CancellationToken ct = default)
        {
            var row = new NotificationOutbox
            {
                OutboxId = ev.EventId,
                EventType = ev.Type,
                PayloadJson = JsonSerializer.Serialize(ev),
                OccurredAtUtc = ev.OccurredAtUtc,
                RetryCount = 0,
                NextAttemptUtc = DateTime.UtcNow
            };
            _db.Outbox.Add(row);
            await _db.SaveChangesAsync(ct);
        }

        public async Task<int> ProcessPendingAsync(CancellationToken ct = default)
            => await _db.Outbox.CountAsync(o => o.ProcessedAtUtc == null && o.NextAttemptUtc <= DateTime.UtcNow, ct);
    }

    public sealed class OutboxWorker : BackgroundService
    {
        private readonly IServiceProvider _sp;
        private readonly IOptions<NotificationsOptions> _opt;

        public OutboxWorker(IServiceProvider sp, IOptions<NotificationsOptions> opt)
        {
            _sp = sp;
            _opt = opt;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _sp.CreateScope();

                    // ✅ Resolve các Scoped service từ scope (KHÔNG giữ trong field)
                    var db = scope.ServiceProvider.GetRequiredService<NotificationsDbContext>();
                    var tpl = scope.ServiceProvider.GetRequiredService<ITemplateRenderer>();
                    var mail = scope.ServiceProvider.GetRequiredService<IEmailSender>();

                    var now = DateTime.UtcNow;
                    var batch = await db.Outbox
                        .Where(o => o.ProcessedAtUtc == null && o.NextAttemptUtc <= now)
                        .OrderBy(o => o.OccurredAtUtc)
                        .Take(_opt.Value.MaxSendPerBatch)
                        .ToListAsync(stoppingToken);

                    foreach (var o in batch)
                    {
                        try
                        {
                            var ev = JsonSerializer.Deserialize<NotificationEvent>(o.PayloadJson)!;

                            var user = await db.Users.AsNoTracking()
                                .SingleOrDefaultAsync(u => u.UserId == ev.TargetUserId, stoppingToken);
                            if (user is null)
                            {
                                o.ProcessedAtUtc = DateTime.UtcNow;
                                db.Deliveries.Add(new NotificationDelivery
                                {
                                    DeliveryId = Guid.NewGuid(),
                                    UserId = ev.TargetUserId,
                                    EventType = ev.Type,
                                    Subject = "Skipped - user not found",
                                    Status = "Skipped",
                                    CreatedAtUtc = DateTime.UtcNow
                                });
                                continue;
                            }

                            var pref = await db.Preferences.AsNoTracking()
                                .SingleOrDefaultAsync(p => p.UserId == user.UserId && p.EventType == ev.Type, stoppingToken);
                            if (pref is null || !pref.EmailEnabled)
                            {
                                o.ProcessedAtUtc = DateTime.UtcNow;
                                db.Deliveries.Add(new NotificationDelivery
                                {
                                    DeliveryId = Guid.NewGuid(),
                                    UserId = ev.TargetUserId,
                                    EventType = ev.Type,
                                    Subject = "Skipped - preference disabled",
                                    Status = "Skipped",
                                    CreatedAtUtc = DateTime.UtcNow
                                });
                                continue;
                            }

                            var model = new
                            {
                                actor = ev.ActorUserId,
                                cardTitle = ev.CardId ?? "(n/a)",
                                cardUrl = $"{_opt.Value.BaseUrl}/c/{ev.CardId ?? ""}",
                                boardName = ev.BoardId ?? "(n/a)",
                                snippet = ev.Description ?? "",
                                commentUrl = $"{_opt.Value.BaseUrl}/c/{ev.CardId ?? ""}#comment-{ev.CommentId ?? ""}",
                                unsubscribeUrl = $"{_opt.Value.BaseUrl}/u/{ev.TargetUserId}/notifications"
                            };

                            var rendered = await tpl.RenderAsync(ev.Type.ToString(), user.Locale, model, stoppingToken);
                            var subject = rendered.Subject;
                            var html = rendered.HtmlBody;
                            var text = rendered.TextBody;

                            var send = await mail.SendAsync(new EmailMessage(user.Email, subject, html, text), stoppingToken);

                            string? preview = ev.Description;
                            if (preview != null && preview.Length > 512) preview = preview.Substring(0, 512);

                            db.Deliveries.Add(new NotificationDelivery
                            {
                                DeliveryId = Guid.NewGuid(),
                                UserId = ev.TargetUserId,
                                EventType = ev.Type,
                                Subject = subject,
                                BodyPreview = preview,
                                Status = send.Success ? "Sent" : "Failed",
                                ProviderMessageId = send.ProviderMessageId,
                                Error = send.Success ? null : send.Error,
                                CreatedAtUtc = DateTime.UtcNow,
                                SentAtUtc = send.Success ? DateTime.UtcNow : null
                            });

                            o.ProcessedAtUtc = DateTime.UtcNow;
                        }
                        catch (Exception ex)
                        {
                            o.RetryCount += 1;
                            o.NextAttemptUtc = DateTime.UtcNow + (o.RetryCount switch
                            {
                                1 => TimeSpan.FromMinutes(1),
                                2 => TimeSpan.FromMinutes(5),
                                3 => TimeSpan.FromMinutes(30),
                                4 => TimeSpan.FromHours(2),
                                _ => TimeSpan.FromHours(6)
                            });

                            db.Deliveries.Add(new NotificationDelivery
                            {
                                DeliveryId = Guid.NewGuid(),
                                UserId = "unknown",
                                EventType = o.EventType,
                                Subject = "Send error",
                                Status = "Failed",
                                Error = ex.Message,
                                CreatedAtUtc = DateTime.UtcNow
                            });
                        }
                    }

                    await db.SaveChangesAsync(stoppingToken);
                }
                catch
                {
                    // log nếu cần
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }


    public sealed class ReminderScheduler : BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.CompletedTask;
    }

    public static class NotificationsServiceCollectionExtensions
    {
        public static IServiceCollection AddNotificationsCore(
            this IServiceCollection services,
            IConfiguration cfg)   
        {
            services.Configure<NotificationsOptions>(cfg.GetSection("Notifications"));

            services.AddDbContext<NotificationsDbContext>(opt =>
                opt.UseSqlServer(cfg.GetConnectionString("NotificationsDb")));

            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<INotificationPublisher, NotificationService>();
            services.AddHostedService<OutboxWorker>();
            services.AddHostedService<ReminderScheduler>();
            return services;
        }
    }

}

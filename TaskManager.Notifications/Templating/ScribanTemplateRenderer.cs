using TaskManager.Notifications.Abstractions;
using TaskManager.Notifications.Persistence.EFCore;
using Scriban;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TaskManager.Notifications.Templating
{
    public sealed class ScribanTemplateRenderer : ITemplateRenderer
    {
        private readonly NotificationsDbContext _db;
        public ScribanTemplateRenderer(NotificationsDbContext db) => _db = db;

        public async Task<(string Subject, string HtmlBody, string? TextBody)> RenderAsync(
            string templateKey, string locale, object model, CancellationToken ct = default)
        {
            var tpl = await _db.Templates.AsNoTracking()
                .SingleOrDefaultAsync(t => t.TemplateKey == templateKey && t.Locale == locale, ct)
                ?? throw new InvalidOperationException($"Template not found: {templateKey}/{locale}");

            var subject = Template.Parse(tpl.SubjectTemplate).Render(model, memberRenamer: m => m.Name);
            var html = Template.Parse(tpl.HtmlTemplate).Render(model, memberRenamer: m => m.Name);
            var text = tpl.TextTemplate is null ? null :
                Template.Parse(tpl.TextTemplate).Render(model, memberRenamer: m => m.Name);

            return (subject, html, text);
        }
    }
}

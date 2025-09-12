using MailKit.Security;
using MimeKit;
using TaskManager.Notifications.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace TaskManager.Notifications.Email.Smtp
{
    public sealed class SmtpOptions
    {
        public string Host { get; set; } = "smtp.gmail.com";
        public int Port { get; set; } = 587;
        public bool UseStartTls { get; set; } = true;
        public string User { get; set; } = "";
        public string Password { get; set; } = "";
        public string From { get; set; } = "";
    }

    public sealed class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpOptions _opt;
        public SmtpEmailSender(IOptions<SmtpOptions> opt) => _opt = opt.Value;

        public async Task<EmailSendResult> SendAsync(EmailMessage m, CancellationToken ct = default)
        {
            try
            {
                var msg = new MimeMessage();
                msg.From.Add(MailboxAddress.Parse(string.IsNullOrWhiteSpace(_opt.From) ? _opt.User : _opt.From));
                msg.To.Add(MailboxAddress.Parse(m.To));
                msg.Subject = m.Subject;
                msg.Body = new BodyBuilder { HtmlBody = m.HtmlBody, TextBody = m.TextBody }.ToMessageBody();

                using var client = new MailKit.Net.Smtp.SmtpClient();
                await client.ConnectAsync(_opt.Host, _opt.Port, _opt.UseStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto, ct);
                if (!string.IsNullOrEmpty(_opt.User))
                    await client.AuthenticateAsync(_opt.User, _opt.Password, ct);
                var resp = await client.SendAsync(msg, ct);
                await client.DisconnectAsync(true, ct);
                return EmailSendResult.Sent(resp);
            }
            catch (Exception ex)
            {
                return EmailSendResult.Failed(ex.Message);
            }
        }
    }
}

using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using HMS.BLL.ServicesAbstraction.Contracts.NotificationService;
using HMS.BLL.Shared.Common.NotificationSettings;

namespace HMS.PL.Senders
{
    public sealed class SmtpEmailSender(IOptions<NotificationEmailSettings> _options) : IEmailSender
    {
        public async Task SendAsync(
            string toEmail,
            string toName,
            string subject,
            string htmlBody,
            byte[]? attachment = null,
            string? attachmentName = null)
        {
            var opts = _options.Value;

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(opts.FromName, opts.FromAddress));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = htmlBody };

            if (attachment is not null && attachmentName is not null)
                builder.Attachments.Add(attachmentName, attachment, new ContentType("application", "pdf"));

            message.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(opts.SmtpHost, opts.SmtpPort, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(opts.SmtpUser, opts.SmtpPassword);
            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);
        }
    }
}
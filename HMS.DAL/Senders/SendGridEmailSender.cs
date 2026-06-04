using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using Services.Abstraction.Contracts.NotificationService;
using Shared.Common.NotificationSettings;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.DAL.Senders
{
    public sealed class SendGridEmailSender(IOptions<NotificationEmailSettings> _options) : IEmailSender
    {
        public async Task SendAsync(
            string toEmail,
            string toName,
            string subject,
            string htmlBody,
            byte[]? attachment = null,
            string? attachmentName = null)
        {
            var client = new SendGridClient(_options.Value.SendGridApiKey);
            var from = new EmailAddress(_options.Value.FromAddress, _options.Value.FromName);
            var to = new EmailAddress(toEmail, toName);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlBody);
 
            if (attachment is not null && attachmentName is not null)
            {
                var base64Content = Convert.ToBase64String(attachment);
                msg.AddAttachment(attachmentName, base64Content, "application/pdf");
            }
 
            var response = await client.SendEmailAsync(msg);
 
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Body.ReadAsStringAsync();
                throw new InvalidOperationException($"SendGrid error {(int)response.StatusCode}: {body}");
            }
        }
    }
}

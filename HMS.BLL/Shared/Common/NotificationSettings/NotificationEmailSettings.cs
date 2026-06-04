using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Shared.Common.NotificationSettings
{
    public class NotificationEmailSettings
    {
        public string Provider { get; set; } = "Smtp"; // "SendGrid" | "Smtp"
        public string SendGridApiKey { get; set; } = string.Empty;
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public string SmtpUser { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public string FromAddress { get; set; } = "no-reply@hms.com";
        public string FromName { get; set; } = "HMS Hospital";
    }
}

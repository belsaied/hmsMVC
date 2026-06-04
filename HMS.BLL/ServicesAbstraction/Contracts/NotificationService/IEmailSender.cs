using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.ServicesAbstraction.Contracts.NotificationService
{
    public interface IEmailSender
    {
        Task SendAsync(string toEmail, string toName, string subject, string htmlBody, byte[]? attachment = null, string? attachmentName = null);
    }
}

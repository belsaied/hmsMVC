using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Services.Abstraction.Contracts;
using HMS.BLL.Shared.Common;


namespace HMS.BLL.Services.Implementations.UserManagementModule
{
    public class EmailService(IOptions<EmailOptions> _options) : IEmailService
    {
        public async Task SendVerificationEmailAsync(string toEmail, string token)
        {
            var link = $"{_options.Value.FrontendUrl}/verify-email?token={token}";
            await SendAsync(toEmail, "Verify your HMS account",
                $"<p>Click the link below to verify your email:</p>" +
                $"<p><a href='{link}'>Verify Email</a></p>" +
                $"<p>This link expires in 24 hours.</p>");
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string token)
        {
            var link = $"{_options.Value.FrontendUrl}/reset-password?token={token}";
            await SendAsync(toEmail, "Reset your HMS password",
                $"<p>Click the link below to reset your password:</p>" +
                $"<p><a href='{link}'>Reset Password</a></p>" +
                $"<p>This link expires in 1 hour.</p>");
        }

        private async Task SendAsync(string to, string subject, string htmlBody)
        {
            var opts = _options.Value;
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(opts.FromAddress));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(opts.SmtpHost, opts.SmtpPort, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(opts.Username, opts.Password);
            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);
        }
        public async Task SendDoctorWelcomeEmailAsync(string toEmail, string doctorName, int doctorId)
        {
            var registerLink = $"{_options.Value.FrontendUrl}/doctor-register";

            await SendAsync(toEmail, "Welcome to HMS — Complete Your Registration",
                $"""
        <h2>Welcome to HMS, Dr. {doctorName}!</h2>
        <p>Your doctor profile has been created in our Hospital Management System.</p>
        
        <div style="background:#f0f4ff;padding:20px;border-radius:8px;margin:20px 0;">
            <h3 style="color:#1E3A5F;">Your Doctor ID</h3>
            <p style="font-size:32px;font-weight:bold;color:#0078D4;letter-spacing:4px;">{doctorId}</p>
            <p>You will need this ID to complete your registration.</p>
        </div>
        
        <h3>Next Steps:</h3>
        <ol>
            <li>Visit the registration page: <a href='{registerLink}'>{registerLink}</a></li>
            <li>Fill in your details and enter your Doctor ID: <strong>{doctorId}</strong></li>
            <li>Set your password</li>
            <li>Verify your email when prompted</li>
        </ol>
        
        <p>If you did not expect this email, please contact the hospital administration.</p>
        <p>Thank you,<br/>HMS Team</p>
        """);
        }

    }

}

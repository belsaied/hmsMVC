using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using HMS.BLL.Shared.Common;
using HMS.BLL.ServicesAbstraction.Contracts;


namespace HMS.BLL.Services.Implementations.UserManagementModule
{
    public class EmailService(IOptions<EmailOptions> _options) : IEmailService
    {
        public async Task SendVerificationEmailAsync(string toEmail, string token)
        {
            var link = $"{_options.Value.FrontendUrl}/Auth/VerifyEmail?token={token}";
            await SendAsync(toEmail, "Verify your HMS account",
                $"<p>Click the link below to verify your email:</p>" +
                $"<p><a href='{link}'>Verify Email</a></p>" +
                $"<p>This link expires in 24 hours.</p>");
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string token)
        {
            var link = $"{_options.Value.FrontendUrl}/Auth/ResetPassword?token={token}";
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
        public async Task SendDoctorWelcomeEmailAsync(string toEmail, string doctorName, int doctorId, string? password = null)
        {
            var registerLink = $"{_options.Value.FrontendUrl}/doctor-register";

            var credentialHtml = string.IsNullOrEmpty(password)
                ? $"""
        <div style="background:#f0f4ff;padding:20px;border-radius:8px;margin:20px 0;">
            <h3 style="color:#1E3A5F;">Your Doctor ID</h3>
            <p style="font-size:32px;font-weight:bold;color:#0078D4;letter-spacing:4px;">{doctorId}</p>
            <p>You will need this ID to complete your registration.</p>
        </div>
        """
                : $"""
        <div style="background:#e8f5e9;padding:20px;border-radius:8px;margin:20px 0;">
            <h3 style="color:#2e7d32;">Your Account Credentials</h3>
            <table style="font-size:14px;line-height:1.8;">
                <tr><td style="font-weight:bold;padding-right:16px;">Doctor ID</td><td style="font-size:20px;font-weight:bold;color:#0078D4;">{doctorId}</td></tr>
                <tr><td style="font-weight:bold;padding-right:16px;">Email</td><td>{toEmail}</td></tr>
                <tr><td style="font-weight:bold;padding-right:16px;">Password</td><td style="font-family:monospace;background:#f5f5f5;padding:2px 8px;border-radius:4px;">{password}</td></tr>
            </table>
            <p style="margin-top:12px;color:#666;font-size:13px;">Please change your password after first login.</p>
        </div>
        """;

            await SendAsync(toEmail, "Welcome to HMS — Your Account Has Been Created",
                $"""
        <h2>Welcome to HMS, Dr. {doctorName}!</h2>
        <p>Your account has been created in our Hospital Management System.</p>
        
        {credentialHtml}
        
        <h3>Getting Started:</h3>
        <ol>
            <li>Visit the login page: <a href='{registerLink}'>{registerLink}</a></li>
            <li>Log in with your <strong>Email</strong> and <strong>Password</strong> above</li>
            <li>Complete your profile and start managing appointments</li>
        </ol>
        
        <p>If you did not expect this email, please contact the hospital administration.</p>
        <p>Thank you,<br/>HMS Team</p>
        """);
        }

    }

}

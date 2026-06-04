namespace HMS.BLL.ServicesAbstraction.Contracts
{
    public interface IEmailService
    {
        Task SendVerificationEmailAsync(string toEmail, string token);
        Task SendPasswordResetEmailAsync(string toEmail, string token);
        Task SendDoctorWelcomeEmailAsync(string toEmail, string doctorName, int doctorId); 

    }
}

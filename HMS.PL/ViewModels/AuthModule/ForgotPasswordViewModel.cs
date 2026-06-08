using System.ComponentModel.DataAnnotations;

namespace HMS.PL.ViewModels.AuthModule
{
    public class ForgotPasswordViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}

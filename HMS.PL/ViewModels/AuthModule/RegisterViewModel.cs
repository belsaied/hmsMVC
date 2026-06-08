using System.ComponentModel.DataAnnotations;

namespace HMS.PL.ViewModels.AuthModule
{
    public class RegisterViewModel
    {
        [Required, MinLength(2), MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MinLength(2), MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(8), DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required, DataType(DataType.Password), Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}

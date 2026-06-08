using System.ComponentModel.DataAnnotations;

namespace HMS.PL.ViewModels.UserManagementModule
{
    public class EditUserViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required, MinLength(2), MaxLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required, MinLength(2), MaxLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
        public string CurrentRole { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Email Verified")]
        public bool IsEmailVerified { get; set; }
    }
}

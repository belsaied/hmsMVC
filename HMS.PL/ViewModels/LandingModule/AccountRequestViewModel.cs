using System.ComponentModel.DataAnnotations;

namespace HMS.PL.ViewModels.LandingModule
{
    public class AccountRequestViewModel
    {
        [Required, MinLength(2), MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string RequestedRole { get; set; } = string.Empty;

        public string? LicenseNumber { get; set; }

        [MaxLength(1000)]
        public string? Message { get; set; }
    }
}

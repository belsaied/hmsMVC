using System.ComponentModel.DataAnnotations;
using HMS.DAL.Models.Enums.PatientEnums;

namespace HMS.PL.ViewModels.UserManagementModule
{
    public class CreateUserViewModel
    {
        [Required, MinLength(2), MaxLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required, MinLength(2), MaxLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(256)]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(8), MaxLength(100)]
        [RegularExpression(@"^(?=.*\d).{8,}$",
            ErrorMessage = "Password must be at least 8 characters and contain at least one digit.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required, Compare(nameof(Password))]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Role")]
        public string Role { get; set; } = string.Empty;

        [Display(Name = "Doctor ID")]
        public int? DoctorId { get; set; }

        [Display(Name = "Phone Number")]
        [Phone, MinLength(11), MaxLength(15)]
        public string? Phone { get; set; }

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Gender")]
        public Gender? Gender { get; set; }

        [Display(Name = "National ID")]
        [MinLength(14), MaxLength(14)]
        public string? NationalId { get; set; }

        public string? Street { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? PostalCode { get; set; }
    }
}

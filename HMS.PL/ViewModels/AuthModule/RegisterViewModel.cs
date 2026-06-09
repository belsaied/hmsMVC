using HMS.DAL.Models.Enums.PatientEnums;
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

        [Required, Phone, MinLength(11), MaxLength(15)]
        public string Phone { get; set; } = string.Empty;

        [Required, Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required]
        public Gender Gender { get; set; }

        [Required, MinLength(14), MaxLength(14)]
        [Display(Name = "National ID")]
        public string NationalId { get; set; } = string.Empty;

        public string? Street { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? PostalCode { get; set; }
    }
}

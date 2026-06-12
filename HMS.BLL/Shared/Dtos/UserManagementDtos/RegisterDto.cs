using HMS.DAL.Models.Enums.PatientEnums;
using System.ComponentModel.DataAnnotations;

namespace HMS.BLL.Shared.Dtos.UserManagementDtos
{
    public record RegisterDto
    {
        [Required, MinLength(2), MaxLength(50),
         RegularExpression(@"^[a-zA-Z\s'\-]+$",
             ErrorMessage = "First name can only contain letters, spaces, hyphens, and apostrophes.")]
        public string FirstName { get; init; } = string.Empty;

        [Required, MinLength(2), MaxLength(50),
         RegularExpression(@"^[a-zA-Z\s'\-]+$",
             ErrorMessage = "Last name can only contain letters, spaces, hyphens, and apostrophes.")]
        public string LastName { get; init; } = string.Empty;

        [Required, EmailAddress, MaxLength(256)]
        public string Email { get; init; } = string.Empty;

        [Required, MinLength(8), MaxLength(100)]
        public string Password { get; init; } = string.Empty;

        [Required]
        [RegularExpression("^(Patient|Doctor|Nurse|Receptionist|HospitalAdmin|SuperAdmin)$",
            ErrorMessage = "Role must be one of: Patient, Doctor, Nurse, Receptionist, HospitalAdmin, SuperAdmin.")]
        public string Role { get; init; } = string.Empty;
        public int? PatientId { get; init; }
        public int? DoctorId { get; init; }
        public PatientRegistrationInfo? PatientInfo { get; init; }
    }

    public record PatientRegistrationInfo : IValidatableObject
    {
        [Required, Phone, MinLength(11), MaxLength(15)]
        public string Phone { get; init; } = string.Empty;

        [Required]
        public DateTime DateOfBirth { get; init; }

        [Required]
        public Gender Gender { get; init; }

        [Required, MinLength(14), MaxLength(14),
         RegularExpression(@"^\d{14}$",
             ErrorMessage = "National ID must be 14 digits and contain numbers only.")]
        public string NationalId { get; init; } = string.Empty;

        [Required]
        public PatientAddressInfo Address { get; init; } = null!;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DateOfBirth >= DateTime.Today)
            {
                yield return new ValidationResult("Date of birth cannot be in the future.", new[] { nameof(DateOfBirth) });
            }
        }
    }

    public record PatientAddressInfo
    {
        public string Street { get; init; } = string.Empty;
        public string City { get; init; } = string.Empty;
        public string Country { get; init; } = string.Empty;
        public string PostalCode { get; init; } = string.Empty;
    }
}

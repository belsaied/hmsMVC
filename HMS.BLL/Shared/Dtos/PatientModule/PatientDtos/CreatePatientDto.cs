using HMS.DAL.Models.Enums.PatientEnums;
using System.ComponentModel.DataAnnotations;

namespace HMS.BLL.Shared.Dtos.PatientModule.PatientDtos
{
    public record CreatePatientDto : IValidatableObject
    {
        [Required, MinLength(2), MaxLength(50),
         RegularExpression(@"^[a-zA-Z\s'\-]+$",
             ErrorMessage = "First name can only contain letters, spaces, hyphens, and apostrophes.")]
        public string FirstName { get; init; } = string.Empty;

        [Required, MinLength(2), MaxLength(50),
         RegularExpression(@"^[a-zA-Z\s'\-]+$",
             ErrorMessage = "Last name can only contain letters, spaces, hyphens, and apostrophes.")]
        public string LastName { get; init; } = string.Empty;

        [Required]
        public DateTime DateOfBirth { get; init; }

        [Required]
        public Gender Gender { get; init; }

        public BloodType? BloodType { get; init; }

        [Required, Phone, MinLength(11), MaxLength(15)]
        public string Phone { get; init; } = string.Empty;

        [Required, MinLength(14), MaxLength(14),
         RegularExpression(@"^\d{14}$",
             ErrorMessage = "National ID must be 14 digits and contain numbers only.")]
        public string NationalId { get; init; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; init; } = string.Empty;

        [Required]
        public AddressDto Address { get; init; } = null!;
        public string? PictureUrl { get; init; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DateOfBirth >= DateTime.Today)
            {
                yield return new ValidationResult("Date of birth cannot be in the future.", new[] { nameof(DateOfBirth) });
            }
        }
    }
}

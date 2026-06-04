using HMS.DAL.Models.Enums.PatientEnums;
using System.ComponentModel.DataAnnotations;

namespace HMS.BLL.Shared.Dtos.PatientModule.PatientDtos
{
    public record CreatePatientDto
    {
        [Required, MinLength(2), MaxLength(50),
         RegularExpression(@"^[\p{L}\s'\-]+$",
             ErrorMessage = "First name can only contain letters, spaces, hyphens, and apostrophes.")]
        public string FirstName { get; init; } = string.Empty;

        [Required, MinLength(2), MaxLength(50),
         RegularExpression(@"^[\p{L}\s'\-]+$",
             ErrorMessage = "Last name can only contain letters, spaces, hyphens, and apostrophes.")]
        public string LastName { get; init; } = string.Empty;

        [Required]
        public DateTime DateOfBirth { get; init; }

        [Required]
        public Gender Gender { get; init; }

        public BloodType? BloodType { get; init; }

        [Required, Phone, MinLength(11), MaxLength(15)]
        public string Phone { get; init; } = string.Empty;

        [Required, MinLength(14), MaxLength(14)]
        public string NationalId { get; init; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; init; } = string.Empty;

        [Required]
        public AddressDto Address { get; init; } = null!;
        public string? PictureUrl { get; init; }
    }
}

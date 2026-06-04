using HMS.DAL.Models.Enums.PatientEnums;
using System.ComponentModel.DataAnnotations;

namespace HMS.BLL.Shared.Dtos.PatientModule.PatientDtos
{
    public record UpdatePatientDto
    {
        [MinLength(2), MaxLength(50),
         RegularExpression(@"^[\p{L}\s'\-]+$",
             ErrorMessage = "First name can only contain letters, spaces, hyphens, and apostrophes.")]
        public string? FirstName { get; init; }

        [MinLength(2), MaxLength(50),
         RegularExpression(@"^[\p{L}\s'\-]+$",
             ErrorMessage = "Last name can only contain letters, spaces, hyphens, and apostrophes.")]
        public string? LastName { get; init; }

        [Phone]
        public string? Phone { get; init; }

        [EmailAddress]
        public string? Email { get; init; }

        public AddressDto? Address { get; init; }

        public PatientStatus? Status { get; init; }
        public string? PictureUrl { get; init; }
    }
}

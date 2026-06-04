using System.ComponentModel.DataAnnotations;

namespace HMS.BLL.Shared.Dtos.PatientModule.EmergencyContactsDtos
{
    public record UpdateEmergencyContactDto
    {
        [MaxLength(100)]
        public string? Name { get; init; }

        [MaxLength(50)]
        public string? Relationship { get; init; }

        [Phone]
        public string? Phone { get; init; }

        [EmailAddress]
        public string? Email { get; init; }

        public bool? IsPrimaryContact { get; init; }

        [MaxLength(500)]
        public string? Notes { get; init; }
    }
}

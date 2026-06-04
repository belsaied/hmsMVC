using System.ComponentModel.DataAnnotations;

namespace HMS.BLL.Shared.Dtos.MedicalRecordsDto
{
    public record CreatePrescriptionDto
    {
        [Required, MaxLength(200)]
        public string MedicationName { get; init; } = string.Empty;

        [Required, MaxLength(50)]
        public string Dosage { get; init; } = string.Empty;

        [Required, MaxLength(100)]
        public string Frequency { get; init; } = string.Empty;

        [Required, Range(1, 365)]
        public int DurationDays { get; init; }

        [MaxLength(500)]
        public string? Instructions { get; init; }

        public bool IsControlledSubstance { get; init; } = false;

        [Required]
        public DateOnly ExpiresAt { get; init; }
    }
}

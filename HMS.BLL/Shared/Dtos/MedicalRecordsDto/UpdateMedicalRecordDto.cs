using System.ComponentModel.DataAnnotations;

namespace HMS.BLL.Shared.Dtos.MedicalRecordsDto
{
    public record UpdateMedicalRecordDto
    {
        [MaxLength(500)]
        public string? ChiefComplaint { get; init; }

        [MaxLength(1000)]
        public string? Diagnosis { get; init; }

        [MaxLength(20)]
        public string? IcdCode { get; init; }

        [MaxLength(4000)]
        public string? ClinicalNotes { get; init; }

        [MaxLength(2000)]
        public string? TreatmentPlan { get; init; }

        public DateOnly? FollowUpDate { get; init; }

        public bool? IsConfidential { get; init; }
    }
}

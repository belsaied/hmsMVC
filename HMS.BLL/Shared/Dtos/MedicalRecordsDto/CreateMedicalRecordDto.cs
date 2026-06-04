using System.ComponentModel.DataAnnotations;

namespace HMS.BLL.Shared.Dtos.MedicalRecordsDto
{
    public record CreateMedicalRecordDto
    {
        [Required]
        public int PatientId { get; init; }

        [Required]
        public int DoctorId { get; init; }

        public int? AppointmentId { get; init; }

        [Required]
        public DateTime VisitDate { get; init; }

        [Required, MaxLength(500)]
        public string ChiefComplaint { get; init; } = string.Empty;

        [Required, MaxLength(1000)]
        public string Diagnosis { get; init; } = string.Empty;

        [MaxLength(20)]
        public string? IcdCode { get; init; }

        [MaxLength(4000)]
        public string? ClinicalNotes { get; init; }

        [MaxLength(2000)]
        public string? TreatmentPlan { get; init; }

        public DateOnly? FollowUpDate { get; init; }

        public bool IsConfidential { get; init; } = false;
    }
}

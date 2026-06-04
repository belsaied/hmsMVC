using HMS.DAL.Models.Enums.PatientEnums;
using System.ComponentModel.DataAnnotations;

namespace HMS.BLL.Shared.Dtos.PatientModule.Medical_History_Dtos
{
    public record CreateMedicalHistoryDto
    {
        [Required]
        public ConditionType ConditionType { get; init; }

        [Required, MaxLength(500)]
        public string Diagnosis { get; init; } = string.Empty;

        [Required]
        public DateTime DiagnosisDate { get; init; }

        [MaxLength(1000)]
        public string? Treatment { get; init; }

        public DateTime? TreatmentStartDate { get; init; }

        [MaxLength(100)]
        public string? RecordedBy { get; init; }

        [MaxLength(2000)]
        public string? Notes { get; init; }
    }
}

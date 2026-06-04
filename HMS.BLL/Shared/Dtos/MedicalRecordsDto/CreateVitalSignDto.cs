using System.ComponentModel.DataAnnotations;

namespace HMS.BLL.Shared.Dtos.MedicalRecordsDto
{
    public record CreateVitalSignDto
    {
        public decimal? Temperature { get; init; }
        public int? BloodPressureSystolic { get; init; }
        public int? BloodPressureDiastolic { get; init; }
        public int? HeartRate { get; init; }
        public int? RespiratoryRate { get; init; }
        public decimal? OxygenSaturation { get; init; }
        public decimal? Weight { get; init; }
        public decimal? Height { get; init; }

        [Required, MaxLength(100)]
        public string RecordedBy { get; init; } = string.Empty;
    }
}

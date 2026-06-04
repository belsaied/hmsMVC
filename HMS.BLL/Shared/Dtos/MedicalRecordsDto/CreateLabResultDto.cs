using System.ComponentModel.DataAnnotations;

namespace HMS.BLL.Shared.Dtos.MedicalRecordsDto
{
    public record CreateLabResultDto
    {
        [Required, MaxLength(4000)]
        public string ResultText { get; init; } = string.Empty;

        public bool IsAbnormal { get; init; } = false;

        [MaxLength(200)]
        public string? NormalRange { get; init; }

        [Required]
        public DateTime PerformedAt { get; init; }

        [Required]
        public DateTime ReportedAt { get; init; }

        [Required, MaxLength(100)]
        public string PerformedBy { get; init; } = string.Empty;
    }
}

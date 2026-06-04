using HMS.DAL.Models.Enums.MedicalRecordEnums;
using System.ComponentModel.DataAnnotations;

namespace HMS.BLL.Shared.Dtos.MedicalRecordsDto
{
    public record CreateLabOrderDto
    {
        [Required, MaxLength(200)]
        public string TestName { get; init; } = string.Empty;

        [MaxLength(50)]
        public string? TestCode { get; init; }

        [Required]
        public LabOrderPriority Priority { get; init; }

        [MaxLength(500)]
        public string? Notes { get; init; }
    }
}

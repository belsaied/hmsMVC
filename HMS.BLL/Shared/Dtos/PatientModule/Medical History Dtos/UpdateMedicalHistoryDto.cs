using System.ComponentModel.DataAnnotations;

namespace HMS.BLL.Shared.Dtos.PatientModule.Medical_History_Dtos
{
    public record UpdateMedicalHistoryDto
    {
        [MaxLength(1000)]
        public string? Treatment { get; init; }

        public DateTime? ResolutionDate { get; init; }

        public bool? IsActive { get; init; }

        [MaxLength(2000)]
        public string? Notes { get; init; }
    }
}

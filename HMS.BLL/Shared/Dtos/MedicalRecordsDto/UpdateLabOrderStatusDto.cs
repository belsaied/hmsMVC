using HMS.DAL.Models.Enums.MedicalRecordEnums;
using System.ComponentModel.DataAnnotations;

namespace HMS.BLL.Shared.Dtos.MedicalRecordsDto
{
    public record UpdateLabOrderStatusDto
    {
        [Required]
        public LabOrderStatus NewStatus { get; init; }
    }
}

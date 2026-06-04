using HMS.DAL.Models.Enums.AppointmentEnums;
using System.ComponentModel.DataAnnotations;

namespace HMS.BLL.Shared.Dtos.AppointmentModule
{
    public record CreateAppointmentDto
    {
        [Required]
        public int PatientId { get; init; }

        [Required]
        public int DoctorId { get; init; }

        [Required]
        public DateOnly AppointmentDate { get; init; }

        [Required]
        public TimeOnly StartTime { get; init; }

        [Required]
        public AppointmentType Type { get; init; }

        [Required, MaxLength(500)]
        public string ReasonForVisit { get; init; } = string.Empty;
    }
}

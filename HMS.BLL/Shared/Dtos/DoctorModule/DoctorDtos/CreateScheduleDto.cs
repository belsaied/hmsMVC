using System.ComponentModel.DataAnnotations;

namespace HMS.BLL.Shared.Dtos.DoctorModule.DoctorDtos
{
    public record CreateScheduleDto
    {
        [Required]
        public DayOfWeek DayOfWeek { get; init; }
        [Required]
        public TimeOnly StartTime { get; init; }

        [Required]
        public TimeOnly EndTime { get; init; }

        [Range(10,120)]
        public int SlotDurationMinutes { get; init; } = 30;

        public bool IsAvailable { get; init; } =true;

        [Range(1,10)]
        public int MaxAppointmentsPerSlot { get; init; } = 1;
    }
}

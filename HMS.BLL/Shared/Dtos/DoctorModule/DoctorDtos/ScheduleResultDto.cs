using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.BLL.Shared.Dtos.DoctorModule.DoctorDtos
{
    public record ScheduleResultDto
    {
        public int Id { get; init; }
        public int DoctorId { get; init; }
        public string DayOfWeek { get; init; } = string.Empty;
        public TimeOnly StartTime { get; init; }
        public TimeOnly EndTime { get; init; }
        public int SlotDurationMinutes { get; init; }
        public bool IsAvailable { get; init; }
        public int MaxAppointmentsPerSlot { get; init; }
    }
}

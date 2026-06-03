namespace HMS.DAL.Models.DoctorModule
{
    public class DoctorSchedule :BaseEntity<int>
    {
        public int DoctorId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public int SlotDurationMinutes { get; set; } = 30;
        public bool IsAvailable { get; set; }
        public int MaxAppointmentsPerSlot { get; set; } = 1;

        #region Navigation Property
        public Doctor Doctor { get; set; } = null!;
        #endregion

    }
}

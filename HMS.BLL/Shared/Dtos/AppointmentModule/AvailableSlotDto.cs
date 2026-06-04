namespace HMS.BLL.Shared.Dtos.AppointmentModule
{
    public record AvailableSlotDto
    {
        public int DoctorId { get; init; }
        public DateOnly Date { get; init; }
        public TimeOnly StartTime { get; init; }
        public TimeOnly EndTime { get; init; }
        public bool IsAvailable { get; init; }
    }
}

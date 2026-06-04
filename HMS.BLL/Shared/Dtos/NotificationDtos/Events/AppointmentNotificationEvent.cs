namespace HMS.BLL.Shared.Dtos.NotificationDtos.Events
{
    public class AppointmentNotificationEvent
    {
        public int AppointmentId { get; set; }
        public int PatientId { get; set; }
        public string PatientEmail { get; set; } = string.Empty!;
        public string? PatientPhone { get; set; }
        public string PatientName { get; set; } = string.Empty!;
        public int DoctorId { get; set; }
        public string DoctorEmail { get; set; } = string.Empty!;
        public string DoctorName { get; set; } = string.Empty!;
        public DateOnly AppointmentDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public string ConfirmationNumber { get; set; } = string.Empty!;
    }
}

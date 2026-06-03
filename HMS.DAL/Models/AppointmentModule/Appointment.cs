using HMS.DAL.Models.DoctorModule;
using HMS.DAL.Models.Enums.AppointmentEnums;
using HMS.DAL.Models.PatientModule;

namespace HMS.DAL.Models.AppointmentModule
{
    public class Appointment : BaseEntity<int>
    {
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public DateOnly AppointmentDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
        public AppointmentType Type { get; set; }
        public string ReasonForVisit { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string? CancellationReason { get; set; }
        public string ConfirmationNumber { get; set; } = string.Empty;
        public DateTime BookedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ReminderSentAt { get; set; }

        // Navigation Properties
        public Patient Patient { get; set; } = null!;
        public Doctor Doctor { get; set; } = null!;
    }
}

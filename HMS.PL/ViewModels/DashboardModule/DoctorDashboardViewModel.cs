using HMS.BLL.Shared.Dtos.AppointmentModule;

namespace HMS.PL.ViewModels.DashboardModule
{
    public class DoctorDashboardViewModel
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string DoctorPictureUrl { get; set; } = string.Empty;

        public int TodayPatientCount { get; set; }
        public int ScheduledTodayCount { get; set; }
        public int ConfirmedTodayCount { get; set; }
        public int CompletedTodayCount { get; set; }

        public int MedicalRecordsWritten { get; set; }

        public List<AppointmentResultDto> TodayAppointments { get; set; } = new();
        public List<DoctorPatientSummaryDto> RecentPatients { get; set; } = new();
    }
}

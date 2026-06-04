using Microsoft.AspNetCore.SignalR;

namespace HMS.PL.Hubs
{
    public class AppointmentHub : Hub
    {
        // Doctors call this to start receiving their appointment events
        public async Task JoinDoctorGroup(int doctorId)
            => await Groups.AddToGroupAsync(Context.ConnectionId, $"doctor-{doctorId}");

        public async Task LeaveDoctorGroup(int doctorId)
            => await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"doctor-{doctorId}");

        // Patients call this to start receiving their appointment events
        public async Task JoinPatientGroup(int patientId)
            => await Groups.AddToGroupAsync(Context.ConnectionId, $"patient-{patientId}");

        public async Task LeavePatientGroup(int patientId)
            => await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"patient-{patientId}");
    }
}

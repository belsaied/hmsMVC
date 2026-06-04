using HMS.BLL.ServicesAbstraction.Contracts;
using Microsoft.AspNetCore.SignalR;

namespace HMS.PL.Hubs
{
    public class AppointmentNotifier (IHubContext<AppointmentHub> _hubContext , ILogger<AppointmentNotifier> _logger): IAppointmentNotifier
    {
        
        public async Task NotifyDoctorAsync(int doctorId, string eventName, object payload)
        {
            try
            {
                await _hubContext.Clients.Group($"doctor-{doctorId}").SendAsync(eventName, payload);
            }
            catch (Exception ex)
            {
                // Log but don't throw — notification failure shouldn't fail the appointment
                _logger.LogWarning(ex, "SignalR notification failed for doctor {DoctorId}", doctorId);
            }
        }

        public async Task NotifyPatientAsync(int patientId, string eventName, object payload)
            => await _hubContext.Clients
                .Group($"patient-{patientId}")
                .SendAsync(eventName, payload);
    }
}

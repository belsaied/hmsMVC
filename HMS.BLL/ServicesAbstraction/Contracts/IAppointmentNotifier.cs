namespace HMS.BLL.ServicesAbstraction.Contracts
{
    public interface IAppointmentNotifier
    {
        Task NotifyDoctorAsync(int doctorId, string eventName, object payload);
        Task NotifyPatientAsync(int patientId, string eventName, object payload);
    }
}

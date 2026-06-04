using HMS.DAL.Models.AppointmentModule;


namespace HMS.BLL.Services.Specifications.AppointmentModule
{
    public class PatientAppointmentsSpecification : BaseSpecifications<Appointment,int>
    {
        public PatientAppointmentsSpecification(int patientId)
    : base(a => a.PatientId == patientId)
        {
            AddInclude(a => a.Doctor);
            AddOrderByDescending(a => a.AppointmentDate);
        }
    }
}

using HMS.DAL.Models.AppointmentModule;


namespace HMS.BLL.Services.Specifications.AppointmentModule
{
    public class DoctorDailyAppointmentsSpecification : BaseSpecifications<Appointment,int>
    {
        public DoctorDailyAppointmentsSpecification(int doctorId, DateOnly date)
    : base(a => a.DoctorId == doctorId && a.AppointmentDate == date)
        {
            AddInclude(a => a.Patient);
            AddOrderBy(a => a.StartTime);
        }
    }
}

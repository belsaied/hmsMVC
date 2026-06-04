using HMS.DAL.Models.AppointmentModule;

namespace HMS.BLL.Services.Specifications.AppointmentModule
{
    public class AppointmentWithDetailsSpecification : BaseSpecifications<Appointment,int>
    {
        public AppointmentWithDetailsSpecification(int id) : base(a => a.Id == id)
        {
            AddInclude(a => a.Patient);
            AddInclude(a => a.Doctor);
        }
    }
}

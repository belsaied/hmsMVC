using HMS.DAL.Models.AppointmentModule;

namespace HMS.BLL.Services.Specifications.AppointmentModule
{
    public class ConfirmationNumberExistsSpecification : BaseSpecifications<Appointment,int>
    {
        public ConfirmationNumberExistsSpecification(string number)
              : base(a => a.ConfirmationNumber == number) { }
    }
}

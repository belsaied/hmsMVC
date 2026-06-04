using HMS.DAL.Models.DoctorModule;

namespace HMS.BLL.Services.Specifications.DoctorModule
{
    public class DoctorByEmailSpecification : BaseSpecifications<Doctor, int>
    {
        public DoctorByEmailSpecification(string email)
    : base(d => d.Email.ToLower() == email.ToLower()) { }
    }
}

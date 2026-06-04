using HMS.DAL.Models.DoctorModule;

namespace HMS.BLL.Services.Specifications.DoctorModule
{
    public class DoctorByLicenseSpecification : BaseSpecifications<Doctor, int>
    {
        public DoctorByLicenseSpecification(string licenseNumber)
    : base(d => d.LicenseNumber == licenseNumber) { }
    }
}

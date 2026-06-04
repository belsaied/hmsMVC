using HMS.DAL.Models.AppointmentModule;
using HMS.BLL.Shared.Parameters;

namespace HMS.BLL.Services.Specifications.AppointmentModule
{
    public class AppointmentCountSpecification : BaseSpecifications<Appointment,int>
    {
        public AppointmentCountSpecification(AppointmentSpecificationParameters p)
    : base(a =>
        (!p.PatientId.HasValue || a.PatientId == p.PatientId.Value) &&
        (!p.DoctorId.HasValue || a.DoctorId == p.DoctorId.Value) &&
        (!p.Status.HasValue || a.Status == p.Status.Value) &&
        (!p.FromDate.HasValue || a.AppointmentDate >= p.FromDate.Value) &&
        (!p.ToDate.HasValue || a.AppointmentDate <= p.ToDate.Value))
        { }
    }
}

using HMS.DAL.Models.AppointmentModule;
using HMS.BLL.Shared.Parameters;

namespace HMS.BLL.Services.Specifications.AppointmentModule
{
    public class AppointmentListSpecification : BaseSpecifications<Appointment,int>
    {
        public AppointmentListSpecification(AppointmentSpecificationParameters p)
    : base(a =>
        (!p.PatientId.HasValue || a.PatientId == p.PatientId.Value) &&
        (!p.DoctorId.HasValue || a.DoctorId == p.DoctorId.Value) &&
        (!p.Status.HasValue || a.Status == p.Status.Value) &&
        (!p.FromDate.HasValue || a.AppointmentDate >= p.FromDate.Value) &&
        (!p.ToDate.HasValue || a.AppointmentDate <= p.ToDate.Value))
        {
            AddInclude(a => a.Patient);
            AddInclude(a => a.Doctor);
            AddOrderBy(a => a.AppointmentDate);
            ApplyPagination(p.PageSize, p.PageIndex);
        }
    }
}

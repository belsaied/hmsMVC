using HMS.DAL.Models.AppointmentModule;
using HMS.DAL.Models.Enums.AppointmentEnums;

namespace HMS.BLL.Services.Specifications.AppointmentModule
{
    public class ConflictCheckSpecification : BaseSpecifications<Appointment,int>
    {
        // Returns any active appointment that overlaps the requested slot
        public ConflictCheckSpecification(int doctorId, DateOnly date,
                                          TimeOnly start, TimeOnly end,
                                          int? excludeId = null)
            : base(a =>
                a.DoctorId == doctorId &&
                a.AppointmentDate == date &&
                (a.Status == AppointmentStatus.Scheduled ||
                 a.Status == AppointmentStatus.Confirmed) &&
                (excludeId == null || a.Id != excludeId.Value) &&
                a.StartTime < end &&   // existing starts before new one ends
                a.EndTime > start)     // existing ends after new one starts
        { }
    }
}

using HMS.DAL.Models.DoctorModule;
using HMS.DAL.Models.Enums.DoctorEnums;


namespace HMS.BLL.Services.Specifications.DoctorModule
{
    public class DoctorAvailableOnDateSpecification:BaseSpecifications<Doctor,int>
    {
        public DoctorAvailableOnDateSpecification(DateTime date)
            : base(BuildCriteria(date))
        {
            AddInclude(d => d.Department);
            AddInclude(d => d.AvailabilitySchedules);
        }

        private static System.Linq.Expressions.Expression<Func<Doctor, bool>> BuildCriteria(DateTime date)
        {
            var day = date.DayOfWeek;
            return d => d.Status == DoctorStatus.Active &&
                        d.AvailabilitySchedules.Any(s =>
                            s.IsAvailable &&
                            s.DayOfWeek == day);
        }
    }
}

using HMS.DAL.Models.DoctorModule;
using HMS.BLL.Shared.Parameters;

namespace HMS.BLL.Services.Specifications.DoctorModule
{
    public class DoctorCountSpecification : BaseSpecifications<Doctor, int>
    {
        public DoctorCountSpecification(DoctorSpecificationParameters parameters)
            : base(BuildCriteria(parameters))
        {
        }

        private static System.Linq.Expressions.Expression<Func<Doctor, bool>> BuildCriteria(DoctorSpecificationParameters parameters)
        {
            var search = parameters.Search;
            var hasStatus = parameters.Status.HasValue;
            var statusVal = parameters.Status.GetValueOrDefault();
            var hasDept = parameters.DepartmentId.HasValue;
            var deptVal = parameters.DepartmentId.GetValueOrDefault();
            var specialization = parameters.Specialization;

            return d =>
                (string.IsNullOrEmpty(search) ||
                 d.FirstName.ToLower().Contains(search != null ? search.ToLower() : "") ||
                 d.LastName.ToLower().Contains(search != null ? search.ToLower() : "")) &&
                (!hasStatus || d.Status == statusVal) &&
                (!hasDept || d.DepartmentId == deptVal) &&
                (string.IsNullOrEmpty(specialization) ||
                 d.Specialization.ToLower().Contains(specialization != null ? specialization.ToLower() : ""));
        }
    }
}

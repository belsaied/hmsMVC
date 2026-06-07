using HMS.BLL.Shared.Dtos.DoctorModule.DepartmentDtos;
using HMS.BLL.Shared.Dtos.DoctorModule.DoctorDtos;

namespace HMS.PL.ViewModels.LandingModule
{
    public class LandingPageViewModel
    {
        public IEnumerable<DepartmentResultDto> Departments { get; set; } = [];
        public IEnumerable<DoctorResultDto> FeaturedDoctors { get; set; } = [];
        public int TotalDoctors { get; set; }
        public int TotalDepartments { get; set; }
    }
}

using HMS.BLL.ServicesAbstraction.Contracts;
using HMS.BLL.Shared.Parameters;
using HMS.DAL.Models.Enums.DoctorEnums;
using HMS.PL.ViewModels;
using HMS.PL.ViewModels.LandingModule;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace HMS.PL.Controllers
{
    public class HomeController : Controller
    {
        private readonly IServiceManager _services;

        public HomeController(IServiceManager services)
        {
            _services = services;
        }

        public async Task<IActionResult> Index()
        {
            var vm = new LandingPageViewModel();

            try
            {
                var departments = await _services.DepartmentService.GetAllDepartmentAsync();
                vm.Departments = departments ?? [];
                vm.TotalDepartments = vm.Departments.Count();

                var doctorResult = await _services.DoctorService.GetAllDoctorsAsync(
                    new DoctorSpecificationParameters
                    {
                        Status = DoctorStatus.Active,
                        PageSize = 6,
                        PageIndex = 1
                    });

                vm.FeaturedDoctors = doctorResult?.Data ?? [];
                vm.TotalDoctors = doctorResult?.TotalCount ?? 0;
            }
            catch
            {
                vm.Departments = [];
                vm.FeaturedDoctors = [];
            }

            return View(vm);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}

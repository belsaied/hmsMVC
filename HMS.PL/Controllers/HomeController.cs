using HMS.BLL.ServicesAbstraction.Contracts;
using HMS.BLL.Shared.Parameters;
using HMS.DAL.Models.Enums.DoctorEnums;
using HMS.PL.ViewModels;
using HMS.PL.ViewModels.LandingModule;
using Microsoft.AspNetCore.Authorization;
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

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult RequestAccount(AccountRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Warning"] = "Please fill in all required fields.";
                return RedirectToAction("Index");
            }

            // In production, persist the request and notify admins
            TempData["Success"] = "Your account request has been submitted. An administrator will review it shortly.";
            return RedirectToAction("Index");
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

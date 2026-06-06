using HMS.BLL.ServicesAbstraction.Contracts;
using HMS.BLL.Shared.Parameters;
using HMS.DAL.Models.Enums.AppointmentEnums;
using HMS.DAL.Models.Enums.DoctorEnums;
using Microsoft.AspNetCore.Mvc;

namespace HMS.PL.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IServiceManager _services;

        public DashboardController(IServiceManager services)
        {
            _services = services;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var patientResult = await _services.PatientService.GetAllPatientsAsync(
                    new PatientSpecificationParameters { PageSize = 1 });
                ViewBag.PatientCount = patientResult.TotalCount;

                var doctorResult = await _services.DoctorService.GetAllDoctorsAsync(
                    new DoctorSpecificationParameters { PageSize = 1, Status = DoctorStatus.Active });
                ViewBag.DoctorCount = doctorResult.TotalCount;

                var today = DateOnly.FromDateTime(DateTime.Today);
                var apptResult = await _services.AppointmentService.GetAllAppointmentsAsync(
                    new AppointmentSpecificationParameters { FromDate = today, ToDate = today, PageSize = 1 });
                ViewBag.AppointmentCount = apptResult.TotalCount;
            }
            catch
            {
                ViewBag.PatientCount = 0;
                ViewBag.DoctorCount = 0;
                ViewBag.AppointmentCount = 0;
            }

            return View();
        }
    }
}

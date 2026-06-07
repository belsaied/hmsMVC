using HMS.BLL.ServicesAbstraction.Contracts;
using HMS.BLL.Shared.Parameters;
using HMS.DAL.Models.Enums.AppointmentEnums;
using HMS.DAL.Models.Enums.BillingEnums;
using HMS.DAL.Models.Enums.DoctorEnums;
using HMS.PL.ViewModels.DashboardModule;
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
            var vm = new AdminDashboardViewModel();

            try
            {
                var patientResult = await _services.PatientService.GetAllPatientsAsync(
                    new PatientSpecificationParameters { PageSize = 1 });
                vm.TotalPatients = patientResult.TotalCount;

                var doctorResult = await _services.DoctorService.GetAllDoctorsAsync(
                    new DoctorSpecificationParameters { PageSize = 1, Status = DoctorStatus.Active });
                vm.ActiveDoctors = doctorResult.TotalCount;

                var today = DateOnly.FromDateTime(DateTime.Today);
                var apptResult = await _services.AppointmentService.GetAllAppointmentsAsync(
                    new AppointmentSpecificationParameters { FromDate = today, ToDate = today, PageSize = 1 });
                vm.TodaysAppointments = apptResult.TotalCount;

                var departments = (await _services.DepartmentService.GetAllDepartmentAsync()).ToList();
                vm.TotalDepartments = departments.Count;

                var wards = (await _services.WardService.GetAllWardsWithOccupancyAsync()).ToList();
                vm.WardOccupancy = wards;
                vm.AvailableBeds = wards.Sum(w => w.AvailableBeds);
                vm.TotalBeds = wards.Sum(w => w.TotalBeds);
                vm.OccupiedBeds = wards.Sum(w => w.OccupiedBeds);

                var admissions = (await _services.AdmissionService.GetActiveAdmissionsAsync()).ToList();
                vm.ActiveAdmissions = admissions.Count;
                vm.RecentAdmissions = admissions.Take(5).ToList();

                var revenueFilters = new ReportFilterParameters
                {
                    StartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-30)),
                    EndDate = DateOnly.FromDateTime(DateTime.Today)
                };
                var revenueReport = await _services.ReportingService.GetRevenueReportAsync(revenueFilters);
                vm.TotalRevenue = revenueReport.TotalRevenue;
                vm.TotalInvoiced = revenueReport.TotalInvoiced;
                vm.TotalOutstanding = revenueReport.TotalOutstanding;
                vm.RevenueByService = revenueReport.ByServiceType;

                var outstanding = await _services.ReportingService.GetOutstandingInvoicesReportAsync();
                vm.OverdueInvoices = outstanding
                    .Where(i => i.Status == InvoiceStatus.Overdue)
                    .Take(5)
                    .ToList();
                vm.OverdueCount = vm.OverdueInvoices.Count;

                vm.ScheduledCount = (await _services.AppointmentService.GetAllAppointmentsAsync(
                    new AppointmentSpecificationParameters { FromDate = today, ToDate = today,
                        Status = AppointmentStatus.Scheduled, PageSize = 1 })).TotalCount;
                vm.ConfirmedCount = (await _services.AppointmentService.GetAllAppointmentsAsync(
                    new AppointmentSpecificationParameters { FromDate = today, ToDate = today,
                        Status = AppointmentStatus.Confirmed, PageSize = 1 })).TotalCount;
                vm.CompletedCount = (await _services.AppointmentService.GetAllAppointmentsAsync(
                    new AppointmentSpecificationParameters { FromDate = today, ToDate = today,
                        Status = AppointmentStatus.Completed, PageSize = 1 })).TotalCount;
                vm.CancelledCount = (await _services.AppointmentService.GetAllAppointmentsAsync(
                    new AppointmentSpecificationParameters { FromDate = today, ToDate = today,
                        Status = AppointmentStatus.Cancelled, PageSize = 1 })).TotalCount;
            }
            catch (Exception ex)
            {
                TempData["DashError"] = "Some dashboard data could not be loaded: " + ex.Message;
            }

            return View(vm);
        }

        public async Task<IActionResult> DoctorDashboard()
        {
            var vm = new DoctorDashboardViewModel();

            try
            {
                var today = DateOnly.FromDateTime(DateTime.Today);

                var userEmail = User.Identity?.Name ?? string.Empty;
                var allDoctors = await _services.DoctorService.GetAllDoctorsAsync(
                    new DoctorSpecificationParameters { PageSize = 200 });
                var myDoctor = allDoctors.Data
                    .FirstOrDefault(d => d.Email?.Equals(userEmail, StringComparison.OrdinalIgnoreCase) == true);

                if (myDoctor is null)
                {
                    TempData["DashError"] = "Doctor profile not found for this account.";
                    return View(vm);
                }

                vm.DoctorId = myDoctor.Id;
                vm.DoctorName = myDoctor.FullName;
                vm.Specialty = myDoctor.Specialization ?? string.Empty;
                vm.DepartmentName = myDoctor.DepartmentName ?? string.Empty;
                vm.DoctorPictureUrl = myDoctor.PictureUrl ?? string.Empty;

                var todayAppts = await _services.AppointmentService.GetAllAppointmentsAsync(
                    new AppointmentSpecificationParameters
                    {
                        DoctorId = myDoctor.Id,
                        FromDate = today,
                        ToDate = today,
                        PageSize = 50
                    });
                vm.TodayAppointments = todayAppts.Data.ToList();
                vm.TodayPatientCount = todayAppts.TotalCount;
                vm.ScheduledTodayCount = todayAppts.Data.Count(a => a.Status == AppointmentStatus.Scheduled.ToString());
                vm.ConfirmedTodayCount = todayAppts.Data.Count(a => a.Status == AppointmentStatus.Confirmed.ToString());
                vm.CompletedTodayCount = todayAppts.Data.Count(a => a.Status == AppointmentStatus.Completed.ToString());

                var myRecords = await _services.MedicalRecordService.GetDoctorMedicalRecordsAsync(
                    myDoctor.Id, new MedicalRecordSpecificationParameters { PageSize = 1 });
                vm.MedicalRecordsWritten = myRecords.TotalCount;

                var myPatients = (await _services.AppointmentService.GetDoctorPatientsAsync(myDoctor.Id)).ToList();
                vm.RecentPatients = myPatients.Take(5).ToList();
            }
            catch (Exception ex)
            {
                TempData["DashError"] = "Some dashboard data could not be loaded: " + ex.Message;
            }

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> GetBedSummary()
        {
            var wards = await _services.WardService.GetAllWardsWithOccupancyAsync();
            return Json(new
            {
                availableBeds = wards.Sum(w => w.AvailableBeds),
                occupiedBeds = wards.Sum(w => w.OccupiedBeds),
                totalBeds = wards.Sum(w => w.TotalBeds)
            });
        }

        [HttpGet]
        public async Task<IActionResult> QuickSearch(string q, string type = "all")
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
                return Json(Array.Empty<object>());

            var results = new List<object>();

            if (type is "all" or "patients")
            {
                var patients = await _services.PatientService.GetAllPatientsAsync(
                    new PatientSpecificationParameters { Search = q, PageSize = 5 });
                foreach (var p in patients.Data)
                {
                    results.Add(new
                    {
                        type = "Patient",
                        id = p.Id,
                        label = p.FullName,
                        subtitle = p.MedicalRecordNumber ?? "Patient",
                        icon = "user-injured",
                        color = "#f59e0b",
                        url = Url.Action("Details", "Patients", new { id = p.Id })
                    });
                }
            }

            if (type is "all" or "doctors")
            {
                var doctors = await _services.DoctorService.GetAllDoctorsAsync(
                    new DoctorSpecificationParameters { Search = q, PageSize = 5 });
                foreach (var d in doctors.Data)
                {
                    results.Add(new
                    {
                        type = "Doctor",
                        id = d.Id,
                        label = d.FullName,
                        subtitle = d.Specialization ?? "Doctor",
                        icon = "user-md",
                        color = "#a78bfa",
                        url = Url.Action("Index", "Doctors", new { search = q })
                    });
                }
            }

            return Json(results);
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboardStats()
        {
            var wards = await _services.WardService.GetAllWardsWithOccupancyAsync();
            var today = DateOnly.FromDateTime(DateTime.Today);

            var appts = await _services.AppointmentService.GetAllAppointmentsAsync(
                new AppointmentSpecificationParameters { FromDate = today, ToDate = today, PageSize = 1 });

            var admissions = await _services.AdmissionService.GetActiveAdmissionsAsync();

            return Json(new
            {
                availableBeds  = wards.Sum(w => w.AvailableBeds),
                occupiedBeds   = wards.Sum(w => w.OccupiedBeds),
                totalBeds      = wards.Sum(w => w.TotalBeds),
                todaysAppts    = appts.TotalCount,
                activeAdmissions = admissions.Count()
            });
        }
    }
}

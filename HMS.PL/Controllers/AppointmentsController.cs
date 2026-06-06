using HMS.BLL.Services.Exceptions;
using HMS.BLL.ServicesAbstraction.Contracts;
using HMS.BLL.Shared;
using HMS.BLL.Shared.Dtos.AppointmentModule;
using HMS.BLL.Shared.Dtos.DoctorModule.DoctorDtos;
using HMS.BLL.Shared.Parameters;
using HMS.DAL.Models.Enums.AppointmentEnums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HMS.PL.Controllers
{
    public class AppointmentsController : Controller
    {
        private readonly IServiceManager _services;

        public AppointmentsController(IServiceManager services)
        {
            _services = services;
        }

        // ── INDEX ────────────────────────────────────────────────────────────────

        public async Task<IActionResult> Index(
            int? patientId, int? doctorId,
            string? status, string? fromDate, string? toDate,
            int pageIndex = 1)
        {
            var parameters = new AppointmentSpecificationParameters
            {
                PatientId = patientId,
                DoctorId = doctorId,
                Status = string.IsNullOrEmpty(status) ? null : Enum.Parse<AppointmentStatus>(status),
                FromDate = string.IsNullOrEmpty(fromDate) ? null : DateOnly.Parse(fromDate),
                ToDate = string.IsNullOrEmpty(toDate) ? null : DateOnly.Parse(toDate),
                PageIndex = pageIndex,
                PageSize = 10
            };

            var result = await _services.AppointmentService.GetAllAppointmentsAsync(parameters);
            var patients = await _services.PatientService.GetAllPatientsAsync(new() { PageSize = 100 });
            var doctors = await _services.DoctorService.GetAllDoctorsAsync(new() { PageSize = 100 });

            ViewBag.PatientId = patientId;
            ViewBag.DoctorId = doctorId;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            ViewBag.Status = status;
            ViewBag.StatusList = BuildEnumSelectList<AppointmentStatus>(status);
            ViewBag.PatientList = new SelectList(patients.Data.Select(p => new { p.Id, Name = p.FullName }), "Id", "Name", patientId);
            ViewBag.DoctorList = new SelectList(doctors.Data.Select(d => new { d.Id, Name = d.FullName }), "Id", "Name", doctorId);

            return View(result);
        }

        // ── DETAILS ─────────────────────────────────────────────────────────────

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var appointment = await _services.AppointmentService.GetAppointmentByIdAsync(id);
                return View(appointment);
            }
            catch (NotFoundException)
            {
                TempData["Error"] = "Appointment not found.";
                return RedirectToAction(nameof(Index));
            }
        }

        // ── BOOK ────────────────────────────────────────────────────────────────

        public async Task<IActionResult> Book()
        {
            await PopulateBookDropdownsAsync();
            return View(new CreateAppointmentDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(CreateAppointmentDto dto)
        {
            if (!ModelState.IsValid)
            {
                await PopulateBookDropdownsAsync();
                return View(dto);
            }

            try
            {
                var created = await _services.AppointmentService.BookAppointmentAsync(dto);
                TempData["Success"] = $"Appointment booked successfully. Confirmation: <code>{created.ConfirmationNumber}</code>";
                return RedirectToAction(nameof(Details), new { id = created.Id });
            }
            catch (BusinessRuleException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch (ConflictException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            await PopulateBookDropdownsAsync();
            return View(dto);
        }

        // ── AVAILABLE SLOTS ─────────────────────────────────────────────────────

        public async Task<IActionResult> AvailableSlots(int doctorId, string date)
        {
            try
            {
                var d = DateOnly.Parse(date);
                var slots = await _services.AppointmentService.GetAvailableSlotsAsync(doctorId, d);
                var doctor = await _services.DoctorService.GetDoctorByIdAsync(doctorId);

                ViewBag.DoctorId = doctorId;
                ViewBag.DoctorName = doctor.FullName;
                ViewBag.Date = date;

                return View(slots);
            }
            catch (NotFoundException)
            {
                TempData["Error"] = "Doctor not found.";
                return RedirectToAction(nameof(Book));
            }
        }

        // ── GET DAILY SCHEDULE (JSON) ───────────────────────────────────────────

        public async Task<IActionResult> GetDailySchedule(int doctorId, string date)
        {
            try
            {
                var d = DateOnly.Parse(date);
                var slots = await _services.AppointmentService.GetAvailableSlotsAsync(doctorId, d);
                var appointments = await _services.AppointmentService.GetDoctorAppointmentsAsync(doctorId, d);
                return Json(new { slots, appointments });
            }
            catch
            {
                return Json(new { slots = Array.Empty<object>(), appointments = Array.Empty<object>() });
            }
        }

        // ── CONFIRM ─────────────────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int id)
        {
            try
            {
                await _services.AppointmentService.ConfirmAppointmentAsync(id);
                TempData["Success"] = "Appointment confirmed.";
            }
            catch (BusinessRuleException ex) { TempData["Error"] = ex.Message; }
            catch (NotFoundException) { TempData["Error"] = "Appointment not found."; }
            catch (Exception ex) { TempData["Error"] = ex.Message; }

            return RedirectToAction(nameof(Details), new { id });
        }

        // ── COMPLETE ────────────────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id, string? notes)
        {
            try
            {
                await _services.AppointmentService.CompleteAppointmentAsync(id, notes);
                TempData["Success"] = "Appointment marked as completed.";
            }
            catch (BusinessRuleException ex) { TempData["Error"] = ex.Message; }
            catch (NotFoundException) { TempData["Error"] = "Appointment not found."; }
            catch (Exception ex) { TempData["Error"] = ex.Message; }

            return RedirectToAction(nameof(Details), new { id });
        }

        // ── CANCEL ──────────────────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id, string reason)
        {
            try
            {
                await _services.AppointmentService.CancelAppointmentAsync(id, reason);
                TempData["Success"] = "Appointment cancelled.";
            }
            catch (BusinessRuleException ex) { TempData["Error"] = ex.Message; }
            catch (NotFoundException) { TempData["Error"] = "Appointment not found."; }
            catch (Exception ex) { TempData["Error"] = ex.Message; }

            return RedirectToAction(nameof(Index));
        }

        // ── UPDATE STATUS ────────────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, UpdateAppointmentStatusDto dto)
        {
            try
            {
                await _services.AppointmentService.UpdateAppointmentStatusAsync(id, dto);
                TempData["Success"] = $"Status updated to {dto.NewStatus}.";
            }
            catch (BusinessRuleException ex) { TempData["Error"] = ex.Message; }
            catch (NotFoundException) { TempData["Error"] = "Appointment not found."; }
            catch (Exception ex) { TempData["Error"] = ex.Message; }

            return RedirectToAction(nameof(Details), new { id });
        }

        // ── PATIENT APPOINTMENTS ─────────────────────────────────────────────────

        public async Task<IActionResult> PatientAppointments(int patientId)
        {
            try
            {
                var patient = await _services.PatientService.GetPatientByIdAsync(patientId);
                var appointments = await _services.AppointmentService.GetPatientAppointmentsAsync(patientId);

                ViewBag.PatientId = patientId;
                ViewBag.PatientName = patient.FullName;
                ViewBag.MRN = patient.MedicalRecordNumber;

                return View(appointments);
            }
            catch (NotFoundException)
            {
                TempData["Error"] = "Patient not found.";
                return RedirectToAction(nameof(Index));
            }
        }

        // ── DOCTOR DAILY APPOINTMENTS ────────────────────────────────────────────

        public async Task<IActionResult> DoctorSchedule(int doctorId, string? date)
        {
            try
            {
                var d = string.IsNullOrEmpty(date) ? DateOnly.FromDateTime(DateTime.Today) : DateOnly.Parse(date);
                var doctor = await _services.DoctorService.GetDoctorByIdAsync(doctorId);
                var list = await _services.AppointmentService.GetDoctorAppointmentsAsync(doctorId, d);

                ViewBag.DoctorId = doctorId;
                ViewBag.DoctorName = doctor.FullName;
                ViewBag.Date = d.ToString("yyyy-MM-dd");

                return View(list);
            }
            catch (NotFoundException)
            {
                TempData["Error"] = "Doctor not found.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET /Appointments/GetDoctorScheduleDays?doctorId=5
        [HttpGet]
        public async Task<IActionResult> GetDoctorScheduleDays(int doctorId)
        {
            try
            {
                var schedules = await _services.DoctorService.GetScheduleAsync(doctorId);
                var availableDays = schedules
                    .Where(s => s.IsAvailable)
                    .Select(s => s.DayOfWeek) // e.g. "Monday", "Wednesday"
                    .ToList();
                return Json(availableDays);
            }
            catch
            {
                return Json(new List<string>());
            }
        }
        // ── HELPERS ──────────────────────────────────────────────────────────────

        private async Task PopulateBookDropdownsAsync()
        {
            var patients = await _services.PatientService.GetAllPatientsAsync(new() { PageSize = 100 });
            ViewBag.PatientList = new SelectList(
                patients.Data.Select(p => new { p.Id, Name = p.FullName }), "Id", "Name");
            ViewBag.TypeList = BuildEnumSelectList<AppointmentType>();

            // Collect all doctors across pages
            var allDoctors = new List<DoctorResultDto>();
            var page = 1;
            PaginatedResult<DoctorResultDto> pageResult;
            do
            {
                pageResult = await _services.DoctorService.GetAllDoctorsAsync(
                    new() { PageIndex = page, PageSize = 20 });
                allDoctors.AddRange(pageResult.Data);
                page++;
            } while (allDoctors.Count < pageResult.TotalCount);

            // Filter to only doctors with at least one available schedule slot
            var filtered = new List<SelectListItem>();
            foreach (var doc in allDoctors)
            {
                var schedules = await _services.DoctorService.GetScheduleAsync(doc.Id);
                if (schedules.Any(s => s.IsAvailable))
                {
                    filtered.Add(new SelectListItem(
                        $"{doc.FullName}|{doc.Specialization}",
                        doc.Id.ToString()));
                }
            }

            ViewBag.DoctorList = new SelectList(filtered, "Value", "Text");
        }

        private static SelectList BuildEnumSelectList<TEnum>(string? selected = null) where TEnum : struct, Enum
        {
            var items = Enum.GetNames(typeof(TEnum))
                .Select(n => new SelectListItem { Value = n, Text = SplitCamelCase(n) })
                .ToList();
            return new SelectList(items, "Value", "Text", selected);
        }

        private static string SplitCamelCase(string s) =>
            System.Text.RegularExpressions.Regex.Replace(s, "(?<=[a-z])(?=[A-Z])", " ");
    }
}
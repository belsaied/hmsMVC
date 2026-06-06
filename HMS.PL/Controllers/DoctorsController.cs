using HMS.BLL.Services.Exceptions;
using HMS.BLL.ServicesAbstraction.Contracts;
using HMS.BLL.Shared;
using HMS.BLL.Shared.Dtos.DoctorModule.DoctorDtos;
using HMS.BLL.Shared.Parameters;
using HMS.DAL.Models.Enums.DoctorEnums;
using HMS.DAL.Models.Enums.PatientEnums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HMS.PL.Controllers
{
    public class DoctorsController : Controller
    {
        private readonly IServiceManager _services;

        public DoctorsController(IServiceManager services)
        {
            _services = services;
        }

        // GET: /Doctors
        public async Task<IActionResult> Index(string? search, string? status,
            string? specialization, int? departmentId, int pageIndex = 1)
        {
            try
            {
                var parameters = new DoctorSpecificationParameters
                {
                    Search = search,
                    Status = string.IsNullOrEmpty(status) ? null : Enum.Parse<DoctorStatus>(status),
                    Specialization = specialization,
                    DepartmentId = departmentId,
                    PageIndex = pageIndex,
                    PageSize = 10
                };

                var result = await _services.DoctorService.GetAllDoctorsAsync(parameters);
                var departments = await _services.DepartmentService.GetAllDepartmentAsync();

                ViewBag.Search = search;
                ViewBag.Status = status;
                ViewBag.Specialization = specialization;
                ViewBag.DepartmentId = departmentId;
                ViewBag.StatusList = GetStatusSelectList(status);
                var allDept = new HMS.BLL.Shared.Dtos.DoctorModule.DepartmentDtos.DepartmentResultDto { Id = 0, Name = "All Departments" };
                ViewBag.DepartmentList = new SelectList(
                    new[] { allDept }.Concat(departments ?? Enumerable.Empty<HMS.BLL.Shared.Dtos.DoctorModule.DepartmentDtos.DepartmentResultDto>()),
                    "Id", "Name", departmentId ?? 0);

                return View(result);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while loading doctors: " + ex.Message;
                return View(new PaginatedResult<DoctorResultDto>(1, 10, 0, Enumerable.Empty<DoctorResultDto>()));
            }
        }

        // GET: /Doctors/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var doctor = await _services.DoctorService.GetDoctorWithDetailsAsync(id);
                return View(doctor);
            }
            catch
            {
                TempData["Error"] = "Doctor not found.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /Doctors/Create
        public async Task<IActionResult> Create()
        {
            await PopulateDropdownsAsync();
            return View(new CreateDoctorDto());
        }

        // POST: /Doctors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateDoctorDto dto)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync();
                return View(dto);
            }

            try
            {
                var created = await _services.DoctorService.RegisterDoctorAsync(dto);
                TempData["Success"] = $"Dr. {created.FullName} registered successfully (ID: {created.Id}).";
                return RedirectToAction(nameof(Details), new { id = created.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateDropdownsAsync();
                return View(dto);
            }
        }

        // GET: /Doctors/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var doctor = await _services.DoctorService.GetDoctorByIdAsync(id);
                var dto = new UpdateDoctorDto
                {
                    FirstName = doctor.FirstName,
                    LastName = doctor.LastName,
                    Phone = doctor.Phone,
                    Email = doctor.Email,
                    Specialization = doctor.Specialization,
                    DepartmentId = doctor.DepartmentId,
                    YearsOfExperience = doctor.YearsOfExperience,
                    ConsultationFee = doctor.ConsultationFee,
                    Bio = doctor.Bio,
                    PictureUrl = doctor.PictureUrl,
                    Status = Enum.Parse<DoctorStatus>(doctor.Status)
                };
                ViewBag.DoctorId = id;
                ViewBag.DoctorName = doctor.FullName;
                await PopulateDropdownsAsync(doctor.Status);
                return View(dto);
            }
            catch
            {
                TempData["Error"] = "Doctor not found.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Doctors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateDoctorDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.DoctorId = id;
                await PopulateDropdownsAsync(dto.Status?.ToString());
                return View(dto);
            }

            try
            {
                await _services.DoctorService.UpdateDoctorAsync(id, dto);
                TempData["Success"] = "Doctor updated successfully.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.DoctorId = id;
                await PopulateDropdownsAsync(dto.Status?.ToString());
                return View(dto);
            }
        }

        // POST: /Doctors/Deactivate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            try
            {
                await _services.DoctorService.DeactivateDoctorAsync(id);
                TempData["Success"] = "Doctor deactivated successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: /Doctors/Schedule/5
        public async Task<IActionResult> Schedule(int id)
        {
            try
            {
                var doctor = await _services.DoctorService.GetDoctorByIdAsync(id);
                var schedules = await _services.DoctorService.GetScheduleAsync(id);
                ViewBag.DoctorId = id;
                ViewBag.DoctorName = doctor.FullName;
                ViewBag.DayOfWeekList = GetDayOfWeekSelectList();
                return View(schedules);
            }
            catch
            {
                TempData["Error"] = "Doctor not found.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Doctors/AddSchedule
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSchedule(int doctorId, CreateScheduleDto dto)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid schedule data. Please check all fields.";
                return RedirectToAction(nameof(Schedule), new { id = doctorId });
            }

            try
            {
                await _services.DoctorService.SetScheduleAsync(doctorId, dto);
                var start = dto.StartTime.ToString("HH:mm");
                var end = dto.EndTime.ToString("HH:mm");
                TempData["Success"] = $"<i class=\"fas fa-check-circle mr-1\"></i> Schedule added for <strong>{dto.DayOfWeek}</strong> ({start} – {end}, {dto.SlotDurationMinutes} min slots)";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Schedule), new { id = doctorId });
        }

        // GET: /Doctors/Qualifications/5
        public async Task<IActionResult> Qualifications(int id)
        {
            try
            {
                var doctor = await _services.DoctorService.GetDoctorWithDetailsAsync(id);
                ViewBag.DoctorId = id;
                ViewBag.DoctorName = doctor.FullName;
                return View(doctor.Qualifications);
            }
            catch
            {
                TempData["Error"] = "Doctor not found.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Doctors/AddQualification
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddQualification(int doctorId, CreateQualificationDto dto)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid qualification data.";
                return RedirectToAction(nameof(Qualifications), new { id = doctorId });
            }

            try
            {
                await _services.DoctorService.AddQualificationAsync(doctorId, dto);
                TempData["Success"] = $"Qualification '{dto.Degree}' added successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Qualifications), new { id = doctorId });
        }

        // POST: /Doctors/RemoveSchedule
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveSchedule(int doctorId, int scheduleId)
        {
            try
            {
                var schedules = await _services.DoctorService.GetScheduleAsync(doctorId);
                var match = schedules.FirstOrDefault(s => s.Id == scheduleId);
                var dayName = match?.DayOfWeek ?? "Unknown";

                await _services.DoctorService.RemoveScheduleAsync(scheduleId);
                TempData["Success"] = $"<i class=\"fas fa-trash-alt mr-1\"></i> Schedule removed for <strong>{dayName}</strong>.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Schedule), new { id = doctorId });
        }

        // GET: /Doctors/Patients/5
        public async Task<IActionResult> Patients(int id)
        {
            try
            {
                var doctor = await _services.DoctorService.GetDoctorByIdAsync(id);
                var patients = await _services.AppointmentService.GetDoctorPatientsAsync(id);
                ViewBag.DoctorId = id;
                ViewBag.DoctorName = doctor.FullName;
                return View(patients);
            }
            catch (NotFoundException)
            {
                TempData["Error"] = "Doctor not found.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /Doctors/GetAvailableDoctors?date=2026-06-06
        [HttpGet]
        public async Task<IActionResult> GetAvailableDoctors(DateTime date)
        {
            try
            {
                var doctors = await _services.DoctorService.GetAvailableDoctorAsync(date);
                return Ok(doctors);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // POST: /Doctors/RemoveQualification
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveQualification(int doctorId, int qualId)
        {
            try
            {
                await _services.DoctorService.RemoveQualificationAsync(doctorId, qualId);
                TempData["Success"] = "Qualification removed.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Qualifications), new { id = doctorId });
        }

        // ── Helpers ───────────────────────────────────────────────────────

        private async Task PopulateDropdownsAsync(string? selectedStatus = null)
        {
            var departments = await _services.DepartmentService.GetAllDepartmentAsync();
            ViewBag.DepartmentList = new SelectList(departments, "Id", "Name");
            ViewBag.GenderList = new SelectList(Enum.GetNames(typeof(Gender)));
            ViewBag.StatusList = GetStatusSelectList(selectedStatus);
        }

        private static SelectList GetStatusSelectList(string? selected = null)
        {
            var items = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "All Statuses" }
            };
            items.AddRange(Enum.GetNames(typeof(DoctorStatus))
                .Select(n => new SelectListItem { Value = n, Text = n }));
            return new SelectList(items, "Value", "Text", selected);
        }

        private static SelectList GetDayOfWeekSelectList()
        {
            var items = Enum.GetNames(typeof(DayOfWeek))
                .Select(n => new SelectListItem { Value = n, Text = n })
                .ToList();
            return new SelectList(items, "Value", "Text");
        }
    }
}
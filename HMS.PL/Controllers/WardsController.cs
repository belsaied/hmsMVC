using HMS.BLL.Services.Exceptions;
using HMS.BLL.ServicesAbstraction.Contracts;
using HMS.BLL.Shared.Dtos.WardBedModule.AdmissionDtos;
using HMS.BLL.Shared.Dtos.WardBedModule.BedDtos;
using HMS.BLL.Shared.Dtos.WardBedModule.RoomsDtos;
using HMS.BLL.Shared.Dtos.WardBedModule.WardDtos;
using HMS.DAL.Models.Enums.WardBedEnums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HMS.PL.Controllers
{
    public class WardsController : Controller
    {
        private readonly IServiceManager _services;

        public WardsController(IServiceManager services)
        {
            _services = services;
        }

        // ── WARDS INDEX ──────────────────────────────────────────────────────────

        public async Task<IActionResult> Index()
        {
            var wards = await _services.WardService.GetAllWardsWithOccupancyAsync();
            return View(wards);
        }

        // ── WARD DETAILS ─────────────────────────────────────────────────────────

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var ward = await _services.WardService.GetWardByIdAsync(id);
                return View(ward);
            }
            catch (NotFoundException)
            {
                TempData["Error"] = "Ward not found.";
                return RedirectToAction(nameof(Index));
            }
        }

        // ── CREATE WARD ──────────────────────────────────────────────────────────

        public IActionResult Create()
        {
            PopulateWardDropdowns();
            return View(new CreateWardDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateWardDto dto)
        {
            if (!ModelState.IsValid)
            {
                PopulateWardDropdowns();
                return View(dto);
            }

            try
            {
                var created = await _services.WardService.CreateWardAsync(dto);
                TempData["Success"] = $"Ward <strong>{created.Name}</strong> created successfully.";
                return RedirectToAction(nameof(Details), new { id = created.Id });
            }
            catch (ConflictException ex) { ModelState.AddModelError(string.Empty, ex.Message); }
            catch (Exception ex) { ModelState.AddModelError(string.Empty, ex.Message); }

            PopulateWardDropdowns();
            return View(dto);
        }

        // ── EDIT WARD ────────────────────────────────────────────────────────────

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var ward = await _services.WardService.GetWardByIdAsync(id);
                var dto = new UpdateWardDto
                {
                    Name = ward.Name,
                    WardType = Enum.Parse<WardType>(ward.WardType),
                    Floor = ward.Floor,
                    PhoneExtension = ward.PhoneExtension,
                    Description = ward.Description,
                    IsActive = ward.IsActive
                };

                ViewBag.WardId = id;
                ViewBag.WardName = ward.Name;
                PopulateWardDropdowns(ward.WardType);
                return View(dto);
            }
            catch (NotFoundException)
            {
                TempData["Error"] = "Ward not found.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateWardDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.WardId = id;
                PopulateWardDropdowns(dto.WardType?.ToString());
                return View(dto);
            }

            try
            {
                await _services.WardService.UpdateWardAsync(id, dto);
                TempData["Success"] = "Ward updated successfully.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (NotFoundException) { TempData["Error"] = "Ward not found."; return RedirectToAction(nameof(Index)); }
            catch (Exception ex) { ModelState.AddModelError(string.Empty, ex.Message); }

            ViewBag.WardId = id;
            PopulateWardDropdowns(dto.WardType?.ToString());
            return View(dto);
        }

        // ── ADD ROOM ─────────────────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRoom(int wardId, CreateRoomDto dto)
        {
            try
            {
                await _services.WardService.AddRoomToWardAsync(wardId, dto);
                TempData["Success"] = $"Room {dto.RoomNumber} added successfully.";
            }
            catch (BusinessRuleException ex) { TempData["Error"] = ex.Message; }
            catch (ConflictException ex) { TempData["Error"] = ex.Message; }
            catch (Exception ex) { TempData["Error"] = ex.Message; }

            return RedirectToAction(nameof(Details), new { id = wardId });
        }

        // ── ADD BED ──────────────────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBed(int wardId, int roomId, CreateBedDto dto)
        {
            try
            {
                await _services.BedService.AddBedToRoomAsync(roomId, dto);
                TempData["Success"] = $"Bed {dto.BedNumber} added successfully.";
            }
            catch (BusinessRuleException ex) { TempData["Error"] = ex.Message; }
            catch (ConflictException ex) { TempData["Error"] = ex.Message; }
            catch (Exception ex) { TempData["Error"] = ex.Message; }

            return RedirectToAction(nameof(Details), new { id = wardId });
        }

        // ── UPDATE BED STATUS ────────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateBedStatus(int wardId, int bedId, UpdateBedStatusDto dto)
        {
            try
            {
                await _services.BedService.UpdateBedStatusAsync(bedId, dto);
                TempData["Success"] = $"Bed status updated to {dto.Status}.";
            }
            catch (BusinessRuleException ex) { TempData["Error"] = ex.Message; }
            catch (Exception ex) { TempData["Error"] = ex.Message; }

            return RedirectToAction(nameof(Details), new { id = wardId });
        }

        // ── AVAILABLE BEDS ───────────────────────────────────────────────────────

        public async Task<IActionResult> AvailableBeds(string? wardType, string? bedType)
        {
            var beds = await _services.BedService.GetAvailableBedsAsync(wardType, bedType);

            ViewBag.WardType = wardType;
            ViewBag.BedType = bedType;
            ViewBag.WardTypeList = BuildEnumSelectList<WardType>(wardType);
            ViewBag.BedTypeList = BuildEnumSelectList<BedType>(bedType);

            return View(beds);
        }

        // ── ADMISSIONS INDEX ─────────────────────────────────────────────────────

        public async Task<IActionResult> Admissions()
        {
            var admissions = await _services.AdmissionService.GetActiveAdmissionsAsync();
            return View(admissions);
        }

        // ── ADMISSION DETAILS ────────────────────────────────────────────────────

        public async Task<IActionResult> AdmissionDetails(int id)
        {
            try
            {
                var admission = await _services.AdmissionService.GetAdmissionByIdAsync(id);
                return View(admission);
            }
            catch (NotFoundException)
            {
                TempData["Error"] = "Admission not found.";
                return RedirectToAction(nameof(Admissions));
            }
        }

        // ── ADMIT PATIENT ────────────────────────────────────────────────────────

        public async Task<IActionResult> Admit()
        {
            await PopulateAdmissionDropdownsAsync();
            return View(new CreateAdmissionDto { AdmissionDate = DateTime.Now });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Admit(CreateAdmissionDto dto)
        {
            if (!ModelState.IsValid)
            {
                await PopulateAdmissionDropdownsAsync();
                return View(dto);
            }

            try
            {
                var admission = await _services.AdmissionService.AdmitPatientAsync(dto);
                TempData["Success"] = $"Patient admitted successfully. Admission ID: {admission.Id}";
                return RedirectToAction(nameof(AdmissionDetails), new { id = admission.Id });
            }
            catch (BusinessRuleException ex) { ModelState.AddModelError(string.Empty, ex.Message); }
            catch (ConflictException ex) { ModelState.AddModelError(string.Empty, ex.Message); }
            catch (Exception ex) { ModelState.AddModelError(string.Empty, ex.Message); }

            await PopulateAdmissionDropdownsAsync();
            return View(dto);
        }

        // ── DISCHARGE ────────────────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Discharge(int admissionId, DischargeDto dto)
        {
            try
            {
                await _services.AdmissionService.DischargePatientAsync(admissionId, dto);
                TempData["Success"] = "Patient discharged successfully.";
            }
            catch (BusinessRuleException ex) { TempData["Error"] = ex.Message; }
            catch (NotFoundException) { TempData["Error"] = "Admission not found."; }
            catch (Exception ex) { TempData["Error"] = ex.Message; }

            return RedirectToAction(nameof(AdmissionDetails), new { id = admissionId });
        }

        // ── TRANSFER BED ─────────────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TransferBed(int admissionId, TransferBedDto dto)
        {
            try
            {
                await _services.AdmissionService.TransferPatientAsync(admissionId, dto);
                TempData["Success"] = "Patient transferred to new bed.";
            }
            catch (BusinessRuleException ex) { TempData["Error"] = ex.Message; }
            catch (NotFoundException) { TempData["Error"] = "Admission or bed not found."; }
            catch (Exception ex) { TempData["Error"] = ex.Message; }

            return RedirectToAction(nameof(AdmissionDetails), new { id = admissionId });
        }

        // ── PATIENT ADMISSION HISTORY ────────────────────────────────────────────

        public async Task<IActionResult> PatientAdmissions(int patientId)
        {
            try
            {
                var patient = await _services.PatientService.GetPatientByIdAsync(patientId);
                var admissions = await _services.AdmissionService.GetPatientAdmissionHistoryAsync(patientId);

                ViewBag.PatientId = patientId;
                ViewBag.PatientName = patient.FullName;
                ViewBag.MRN = patient.MedicalRecordNumber;

                return View(admissions);
            }
            catch (NotFoundException)
            {
                TempData["Error"] = "Patient not found.";
                return RedirectToAction(nameof(Admissions));
            }
        }

        // ── HELPERS ──────────────────────────────────────────────────────────────

        private void PopulateWardDropdowns(string? selectedWardType = null)
        {
            ViewBag.WardTypeList = BuildEnumSelectList<WardType>(selectedWardType);
        }

        private async Task PopulateAdmissionDropdownsAsync()
        {
            var patients = await _services.PatientService.GetAllPatientsAsync(new() { PageSize = 200 });
            var doctors = await _services.DoctorService.GetAllDoctorsAsync(new() { PageSize = 200 });
            var beds = await _services.BedService.GetAvailableBedsAsync();

            ViewBag.PatientList = new SelectList(patients.Data.Select(p => new { p.Id, Name = $"{p.FullName} ({p.MedicalRecordNumber})" }), "Id", "Name");
            ViewBag.DoctorList = new SelectList(doctors.Data.Select(d => new { d.Id, Name = $"Dr. {d.FullName}" }), "Id", "Name");
            ViewBag.BedList = new SelectList(beds.Select(b => new
            {
                b.BedId,
                Label = $"{b.WardName} / Room {b.RoomNumber} / Bed {b.BedNumber} ({b.BedType})"
            }), "BedId", "Label");
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
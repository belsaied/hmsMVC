using HMS.BLL.ServicesAbstraction.Contracts;
using HMS.BLL.Services.Exceptions;
using HMS.BLL.Shared.Dtos.PatientModule.PatientDtos;
using HMS.BLL.Shared.Dtos.PatientModule.AllergyDtos;
using HMS.BLL.Shared.Dtos.PatientModule.EmergencyContactsDtos;
using HMS.BLL.Shared.Dtos.PatientModule.Medical_History_Dtos;
using HMS.BLL.Shared.Dtos.MedicalRecordsDto;
using HMS.BLL.Shared.Parameters;
using HMS.DAL.Models.Enums.PatientEnums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HMS.PL.Controllers
{
    public class PatientsController : Controller
    {
        private readonly IServiceManager _services;
        private readonly IWebHostEnvironment _env;

        public PatientsController(IServiceManager services, IWebHostEnvironment env)
        {
            _services = services;
            _env = env;
        }

        // ── LIST ────────────────────────────────────────────────────────────────

        public async Task<IActionResult> Index(string? search, string? status, int pageIndex = 1)
        {
            var parameters = new PatientSpecificationParameters
            {
                Search = search,
                Status = string.IsNullOrEmpty(status) ? null : Enum.Parse<PatientStatus>(status),
                PageIndex = pageIndex,
                PageSize = 12
            };

            var result = await _services.PatientService.GetAllPatientsAsync(parameters);

            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.StatusList = BuildStatusSelectList(status);

            return View(result);
        }

        // ── DETAILS ─────────────────────────────────────────────────────────────

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var patient = await _services.PatientService.GetPatientWithDetailsAsync(id);
                return View(patient);
            }
            catch (NotFoundException)
            {
                TempData["Error"] = "Patient not found.";
                return RedirectToAction(nameof(Index));
            }
        }

        // ── CREATE ──────────────────────────────────────────────────────────────

        public IActionResult Create()
        {
            PopulateDropdowns();
            return View(new CreatePatientDto { Address = new AddressDto() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePatientDto dto)
        {
            if (!ModelState.IsValid)
            {
                PopulateDropdowns();
                return View(dto);
            }

            try
            {
                var created = await _services.PatientService.RegisterPatientAsync(dto);
                TempData["Success"] = $"Patient <strong>{created.FullName}</strong> registered successfully. MRN: <code>{created.MedicalRecordNumber}</code>";
                return RedirectToAction(nameof(Details), new { id = created.Id });
            }
            catch (ConflictException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            PopulateDropdowns();
            return View(dto);
        }

        // ── EDIT ────────────────────────────────────────────────────────────────

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var patient = await _services.PatientService.GetPatientByIdAsync(id);
                var dto = new UpdatePatientDto
                {
                    FirstName = patient.FirstName,
                    LastName = patient.LastName,
                    Phone = patient.Phone,
                    Email = patient.Email,
                    PictureUrl = patient.PictureUrl,
                    Address = patient.Address,
                    Status = Enum.Parse<PatientStatus>(patient.Status)
                };

                ViewBag.PatientId = id;
                ViewBag.PatientName = patient.FullName;
                ViewBag.CurrentPicture = patient.PictureUrl;
                PopulateDropdowns(patient.Status);
                return View(dto);
            }
            catch (NotFoundException)
            {
                TempData["Error"] = "Patient not found.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdatePatientDto dto, IFormFile? pictureFile)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.PatientId = id;
                PopulateDropdowns(dto.Status?.ToString());
                return View(dto);
            }

            try
            {
                // Handle picture upload
                if (pictureFile is { Length: > 0 })
                {
                    var result = await SavePictureAsync(pictureFile, "patients");
                    if (result.IsError)
                    {
                        ModelState.AddModelError("PictureFile", result.ErrorMessage!);
                        ViewBag.PatientId = id;
                        PopulateDropdowns(dto.Status?.ToString());
                        return View(dto);
                    }
                    dto = dto with { PictureUrl = result.Path };
                }

                await _services.PatientService.UpdatePatientAsync(id, dto);
                TempData["Success"] = "Patient updated successfully.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (NotFoundException)
            {
                TempData["Error"] = "Patient not found.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.PatientId = id;
                PopulateDropdowns(dto.Status?.ToString());
                return View(dto);
            }
        }

        // ── DEACTIVATE ──────────────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            try
            {
                await _services.PatientService.DeactivatePatientAsync(id);
                TempData["Success"] = "Patient deactivated successfully.";
            }
            catch (BusinessRuleException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (NotFoundException)
            {
                TempData["Error"] = "Patient not found.";
            }
            return RedirectToAction(nameof(Index));
        }

        // ── ALLERGIES ───────────────────────────────────────────────────────────

        public async Task<IActionResult> Allergies(int id)
        {
            try
            {
                var patient = await _services.PatientService.GetPatientByIdAsync(id);
                var allergies = await _services.AllergyService.GetPatientAllergiesAsync(id);

                ViewBag.PatientId = id;
                ViewBag.PatientName = patient.FullName;
                ViewBag.PatientStatus = patient.Status;
                ViewBag.MedicalRecordNumber = patient.MedicalRecordNumber;
                ViewBag.AllergyTypeList = BuildEnumSelectList<AllergyType>();
                ViewBag.SeverityList = BuildEnumSelectList<Severity>();

                return View(allergies);
            }
            catch (NotFoundException)
            {
                TempData["Error"] = "Patient not found.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAllergy(int id, CreateAllergyDto dto)
        {
            try
            {
                await _services.AllergyService.AddAllergyAsync(id, dto);
                TempData["Success"] = $"Allergy '{dto.Type}' recorded successfully.";
            }
            catch (BusinessRuleException ex) { TempData["Error"] = ex.Message; }
            catch (NotFoundException) { TempData["Error"] = "Patient not found."; }
            catch (Exception ex) { TempData["Error"] = ex.Message; }

            return RedirectToAction(nameof(Allergies), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveAllergy(int id, int allergyId)
        {
            try
            {
                await _services.AllergyService.RemoveAllergyAsync(id, allergyId);
                TempData["Success"] = "Allergy removed successfully.";
            }
            catch (BusinessRuleException ex) { TempData["Error"] = ex.Message; }
            catch (Exception ex) { TempData["Error"] = ex.Message; }

            return RedirectToAction(nameof(Allergies), new { id });
        }

        // ── MEDICAL HISTORIES ───────────────────────────────────────────────────

        public async Task<IActionResult> MedicalHistories(int id)
        {
            try
            {
                var patient = await _services.PatientService.GetPatientByIdAsync(id);
                var histories = await _services.MedicalHistoryService.GetPatientMedicalHistoryAsync(id);

                ViewBag.PatientId = id;
                ViewBag.PatientName = patient.FullName;
                ViewBag.PatientStatus = patient.Status;
                ViewBag.MedicalRecordNumber = patient.MedicalRecordNumber;
                ViewBag.ConditionTypeList = BuildEnumSelectList<ConditionType>();

                return View(histories);
            }
            catch (NotFoundException)
            {
                TempData["Error"] = "Patient not found.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMedicalHistory(int id, CreateMedicalHistoryDto dto)
        {
            try
            {
                await _services.MedicalHistoryService.AddMedicalHistoryAsync(id, dto);
                TempData["Success"] = "Medical history entry added successfully.";
            }
            catch (BusinessRuleException ex) { TempData["Error"] = ex.Message; }
            catch (Exception ex) { TempData["Error"] = ex.Message; }

            return RedirectToAction(nameof(MedicalHistories), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateMedicalHistory(int id, int historyId, UpdateMedicalHistoryDto dto)
        {
            try
            {
                await _services.MedicalHistoryService.UpdateMedicalHistoryAsync(id, historyId, dto);
                TempData["Success"] = "Medical history updated successfully.";
            }
            catch (BusinessRuleException ex) { TempData["Error"] = ex.Message; }
            catch (Exception ex) { TempData["Error"] = ex.Message; }

            return RedirectToAction(nameof(MedicalHistories), new { id });
        }

        // ── EMERGENCY CONTACTS ──────────────────────────────────────────────────

        public async Task<IActionResult> EmergencyContacts(int id)
        {
            try
            {
                var patient = await _services.PatientService.GetPatientByIdAsync(id);
                var contacts = await _services.EmergencyContactService.GetEmergencyContactsAsync(id);

                ViewBag.PatientId = id;
                ViewBag.PatientName = patient.FullName;
                ViewBag.PatientStatus = patient.Status;
                ViewBag.MedicalRecordNumber = patient.MedicalRecordNumber;

                return View(contacts);
            }
            catch (NotFoundException)
            {
                TempData["Error"] = "Patient not found.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEmergencyContact(int id, CreateEmergencyContactDto dto)
        {
            try
            {
                await _services.EmergencyContactService.AddEmergencyContactAsync(id, dto);
                TempData["Success"] = $"Emergency contact '{dto.Name}' added successfully.";
            }
            catch (BusinessRuleException ex) { TempData["Error"] = ex.Message; }
            catch (Exception ex) { TempData["Error"] = ex.Message; }

            return RedirectToAction(nameof(EmergencyContacts), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateEmergencyContact(int id, int contactId, UpdateEmergencyContactDto dto)
        {
            try
            {
                await _services.EmergencyContactService.UpdateEmergencyContactAsync(id, contactId, dto);
                TempData["Success"] = "Emergency contact updated successfully.";
            }
            catch (BusinessRuleException ex) { TempData["Error"] = ex.Message; }
            catch (Exception ex) { TempData["Error"] = ex.Message; }

            return RedirectToAction(nameof(EmergencyContacts), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEmergencyContact(int id, int contactId)
        {
            try
            {
                await _services.EmergencyContactService.DeleteEmergencyContactAsync(id, contactId);
                TempData["Success"] = "Emergency contact removed successfully.";
            }
            catch (BusinessRuleException ex) { TempData["Error"] = ex.Message; }
            catch (Exception ex) { TempData["Error"] = ex.Message; }

            return RedirectToAction(nameof(EmergencyContacts), new { id });
        }

        // ── VITALS ──────────────────────────────────────────────────────────────

        public async Task<IActionResult> Vitals(int id)
        {
            try
            {
                var patient = await _services.PatientService.GetPatientByIdAsync(id);
                var vitals = await _services.VitalSignService.GetPatientVitalHistoryAsync(id);
                var latest = await _services.VitalSignService.GetLatestVitalsAsync(id);

                ViewBag.PatientId = id;
                ViewBag.PatientName = patient.FullName;
                ViewBag.PatientStatus = patient.Status;
                ViewBag.MedicalRecordNumber = patient.MedicalRecordNumber;
                ViewBag.LatestVitals = latest;

                return View(vitals);
            }
            catch (NotFoundException)
            {
                TempData["Error"] = "Patient not found.";
                return RedirectToAction(nameof(Index));
            }
        }

        // ── PRESCRIPTIONS ───────────────────────────────────────────────────────

        public async Task<IActionResult> Prescriptions(int id)
        {
            try
            {
                var patient = await _services.PatientService.GetPatientByIdAsync(id);
                var prescriptions = await _services.PrescriptionService.GetPatientPrescriptionsAsync(id);

                ViewBag.PatientId = id;
                ViewBag.PatientName = patient.FullName;
                ViewBag.PatientStatus = patient.Status;
                ViewBag.MedicalRecordNumber = patient.MedicalRecordNumber;

                return View(prescriptions);
            }
            catch (NotFoundException)
            {
                TempData["Error"] = "Patient not found.";
                return RedirectToAction(nameof(Index));
            }
        }

        // ── LAB ORDERS ──────────────────────────────────────────────────────────

        public async Task<IActionResult> LabOrders(int id)
        {
            try
            {
                var patient = await _services.PatientService.GetPatientByIdAsync(id);
                var orders = await _services.LabOrderService.GetPatientLabOrdersAsync(id);

                ViewBag.PatientId = id;
                ViewBag.PatientName = patient.FullName;
                ViewBag.PatientStatus = patient.Status;
                ViewBag.MedicalRecordNumber = patient.MedicalRecordNumber;

                return View(orders);
            }
            catch (NotFoundException)
            {
                TempData["Error"] = "Patient not found.";
                return RedirectToAction(nameof(Index));
            }
        }

        // ── PRESCRIPTION PDF ────────────────────────────────────────────────────

        public async Task<IActionResult> DownloadPrescriptionPdf(int id, int prescriptionId)
        {
            try
            {
                var pdfBytes = await _services.PrescriptionService.GeneratePrescriptionPdfAsync(prescriptionId, id);
                return File(pdfBytes, "application/pdf", $"Prescription_{prescriptionId}.pdf");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to generate PDF: " + ex.Message;
                return RedirectToAction(nameof(Prescriptions), new { id });
            }
        }

        // ── PICTURE UPLOAD ──────────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadPicture(int id, IFormFile file)
        {
            try
            {
                if (file is null || file.Length == 0)
                {
                    TempData["Error"] = "Please select a file.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                var result = await SavePictureAsync(file, "patients");
                if (result.IsError)
                {
                    TempData["Error"] = result.ErrorMessage;
                    return RedirectToAction(nameof(Details), new { id });
                }

                await _services.PatientService.UpdatePatientAsync(id, new UpdatePatientDto { PictureUrl = result.Path });
                TempData["Success"] = "Profile picture updated successfully.";
            }
            catch (Exception ex) { TempData["Error"] = ex.Message; }

            return RedirectToAction(nameof(Details), new { id });
        }

        // ── PRIVATE HELPERS ─────────────────────────────────────────────────────

        private void PopulateDropdowns(string? selectedStatus = null)
        {
            ViewBag.GenderList = BuildEnumSelectList<Gender>();
            ViewBag.BloodTypeList = BuildEnumSelectList<BloodType>(addEmpty: true);
            ViewBag.StatusList = BuildStatusSelectList(selectedStatus);
        }

        private static SelectList BuildStatusSelectList(string? selected = null)
        {
            var items = Enum.GetNames(typeof(PatientStatus))
                .Select(n => new SelectListItem { Value = n, Text = n })
                .ToList();
            return new SelectList(items, "Value", "Text", selected);
        }

        private static SelectList BuildEnumSelectList<TEnum>(bool addEmpty = false) where TEnum : struct, Enum
        {
            var items = Enum.GetNames(typeof(TEnum))
                .Select(n => new SelectListItem { Value = n, Text = SplitCamelCase(n) })
                .ToList();

            if (addEmpty)
                items.Insert(0, new SelectListItem { Value = "", Text = "— Not specified —" });

            return new SelectList(items, "Value", "Text");
        }

        private static string SplitCamelCase(string s) =>
            System.Text.RegularExpressions.Regex.Replace(s, "(?<=[a-z])(?=[A-Z])", " ");

        private async Task<(bool IsError, string? ErrorMessage, string? Path)> SavePictureAsync(
            IFormFile file, string subfolder)
        {
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowed.Contains(ext))
                return (true, "Only JPG, PNG, and WebP images are allowed.", null);

            if (file.Length > 5 * 1024 * 1024)
                return (true, "Image must be under 5 MB.", null);

            var folder = Path.Combine(_env.WebRootPath, "images", subfolder);
            Directory.CreateDirectory(folder);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var fullPath = Path.Combine(folder, fileName);

            await using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            return (false, null, $"images/{subfolder}/{fileName}");
        }
    }
}
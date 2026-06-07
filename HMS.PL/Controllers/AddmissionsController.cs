using HMS.BLL.Services.Exceptions;
using HMS.BLL.ServicesAbstraction.Contracts;
using HMS.BLL.Shared.Dtos.WardBedModule.AdmissionDtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HMS.PL.Controllers
{
    public class AdmissionsController : Controller
    {
        private readonly IServiceManager _services;

        public AdmissionsController(IServiceManager services)
        {
            _services = services;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var admissions = await _services.AdmissionService.GetActiveAdmissionsAsync();
                return View("~/Views/WardBeds/Admissions.cshtml", admissions);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View("~/Views/WardBeds/Admissions.cshtml", Enumerable.Empty<AdmissionResultDto>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var admission = await _services.AdmissionService.GetAdmissionByIdAsync(id);
                return View("~/Views/WardBeds/AdmissionDetails.cshtml", admission);
            }
            catch (NotFoundException)
            {
                TempData["Error"] = "Admission not found.";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Admit()
        {
            try
            {
                await PopulateAdmissionDropdownsAsync();
                return View("~/Views/WardBeds/Admit.cshtml", new CreateAdmissionDto { AdmissionDate = DateTime.Now });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Admit(CreateAdmissionDto dto)
        {
            if (!ModelState.IsValid)
            {
                await PopulateAdmissionDropdownsAsync();
                return View("~/Views/WardBeds/Admit.cshtml", dto);
            }

            try
            {
                var admission = await _services.AdmissionService.AdmitPatientAsync(dto);
                TempData["Success"] = $"Patient admitted successfully. Admission ID: {admission.Id}";
                return RedirectToAction(nameof(Details), new { id = admission.Id });
            }
            catch (BusinessRuleException ex) { ModelState.AddModelError(string.Empty, ex.Message); }
            catch (ConflictException ex) { ModelState.AddModelError(string.Empty, ex.Message); }
            catch (Exception ex) { ModelState.AddModelError(string.Empty, ex.Message); }

            await PopulateAdmissionDropdownsAsync();
            return View("~/Views/WardBeds/Admit.cshtml", dto);
        }

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

            return RedirectToAction(nameof(Details), new { id = admissionId });
        }

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

            return RedirectToAction(nameof(Details), new { id = admissionId });
        }

        public async Task<IActionResult> PatientHistory(int patientId)
        {
            try
            {
                var patient = await _services.PatientService.GetPatientByIdAsync(patientId);
                var admissions = await _services.AdmissionService.GetPatientAdmissionHistoryAsync(patientId);

                ViewBag.PatientId = patientId;
                ViewBag.PatientName = patient.FullName;
                ViewBag.MRN = patient.MedicalRecordNumber;

                return View("~/Views/WardBeds/PatientAdmissions.cshtml", admissions);
            }
            catch (NotFoundException)
            {
                TempData["Error"] = "Patient not found.";
                return RedirectToAction(nameof(Index));
            }
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
    }
}

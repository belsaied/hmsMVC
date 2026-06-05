using HMS.BLL.Services.Exceptions;
using HMS.BLL.ServicesAbstraction.Contracts;
using HMS.BLL.Shared;
using HMS.BLL.Shared.Dtos.MedicalRecordsDto;
using HMS.BLL.Shared.Parameters;
using HMS.DAL.Models.Enums.MedicalRecordEnums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HMS.PL.Controllers
{
    public class MedicalRecordsController : Controller
    {
        private readonly IServiceManager _services;

        public MedicalRecordsController(IServiceManager services)
        {
            _services = services;
        }

        public async Task<IActionResult> Index(int? patientId, int? doctorId, int pageIndex = 1)
        {
            try
            {
                var parameters = new MedicalRecordSpecificationParameters
                {
                    PageIndex = pageIndex,
                    PageSize = 10
                };

                PaginatedResult<MedicalRecordResultDto>? result = null;

                if (patientId.HasValue)
                {
                    ViewBag.FilterLabel = "Patient";
                    try
                    {
                        var patient = await _services.PatientService.GetPatientByIdAsync(patientId.Value);
                        ViewBag.FilterValue = patient.FullName;
                        ViewBag.PatientId = patientId;
                    }
                    catch (NotFoundException)
                    {
                        TempData["Error"] = $"Patient with ID {patientId} not found.";
                        ViewBag.PatientId = patientId;
                        ViewBag.PageIndex = pageIndex;
                        ViewBag.DoctorId = doctorId;
                        return View(new PaginatedResult<MedicalRecordResultDto>(1, 10, 0, Enumerable.Empty<MedicalRecordResultDto>()));
                    }
                    result = await _services.MedicalRecordService.GetPatientMedicalRecordsAsync(patientId.Value, parameters);
                }
                else if (doctorId.HasValue)
                {
                    ViewBag.FilterLabel = "Doctor";
                    try
                    {
                        var doctor = await _services.DoctorService.GetDoctorByIdAsync(doctorId.Value);
                        ViewBag.FilterValue = doctor.FullName;
                        ViewBag.DoctorId = doctorId;
                    }
                    catch (NotFoundException)
                    {
                        TempData["Error"] = $"Doctor with ID {doctorId} not found.";
                        ViewBag.DoctorId = doctorId;
                        ViewBag.PageIndex = pageIndex;
                        ViewBag.PatientId = patientId;
                        return View(new PaginatedResult<MedicalRecordResultDto>(1, 10, 0, Enumerable.Empty<MedicalRecordResultDto>()));
                    }
                    result = await _services.MedicalRecordService.GetDoctorMedicalRecordsAsync(doctorId.Value, parameters);
                }

                ViewBag.PageIndex = pageIndex;
                ViewBag.PatientId = patientId;
                ViewBag.DoctorId = doctorId;
                return View(result ?? new PaginatedResult<MedicalRecordResultDto>(1, 10, 0, Enumerable.Empty<MedicalRecordResultDto>()));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred: " + ex.Message;
                return View(new PaginatedResult<MedicalRecordResultDto>(1, 10, 0, Enumerable.Empty<MedicalRecordResultDto>()));
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var record = await _services.MedicalRecordService.GetMedicalRecordByIdAsync(id);
                ViewBag.PriorityList = new SelectList(Enum.GetNames(typeof(LabOrderPriority)));
                ViewBag.StatusList = new SelectList(Enum.GetNames(typeof(LabOrderStatus)));
                return View(record);
            }
            catch (NotFoundException)
            {
                TempData["Error"] = "Medical record not found.";
                return RedirectToAction("Index", "Patients");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index", "Patients");
            }
        }

        public async Task<IActionResult> Create(int? patientId, int? doctorId)
        {
            try
            {
                if (patientId.HasValue)
                {
                    var patient = await _services.PatientService.GetPatientByIdAsync(patientId.Value);
                    ViewBag.PatientId = patientId;
                    ViewBag.PatientName = patient.FullName;
                }
                if (doctorId.HasValue)
                {
                    var doctor = await _services.DoctorService.GetDoctorByIdAsync(doctorId.Value);
                    ViewBag.DoctorId = doctorId;
                    ViewBag.DoctorName = doctor.FullName;
                }
                return View(new CreateMedicalRecordDto
                {
                    VisitDate = DateTime.Today,
                    PatientId = patientId ?? 0,
                    DoctorId = doctorId ?? 0
                });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index", "Patients");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateMedicalRecordDto dto)
        {
            if (!ModelState.IsValid)
            {
                if (dto.PatientId > 0)
                {
                    try
                    {
                        var p = await _services.PatientService.GetPatientByIdAsync(dto.PatientId);
                        ViewBag.PatientName = p.FullName;
                    }
                    catch { }
                }
                if (dto.DoctorId > 0)
                {
                    try
                    {
                        var d = await _services.DoctorService.GetDoctorByIdAsync(dto.DoctorId);
                        ViewBag.DoctorName = d.FullName;
                    }
                    catch { }
                }
                return View(dto);
            }

            try
            {
                var created = await _services.MedicalRecordService.CreateMedicalRecordAsync(dto);
                TempData["Success"] = $"Medical record created for patient visit on {created.VisitDate:dd MMM yyyy}.";
                return RedirectToAction(nameof(Details), new { id = created.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var record = await _services.MedicalRecordService.GetMedicalRecordByIdAsync(id);
                var dto = new UpdateMedicalRecordDto
                {
                    ChiefComplaint = record.ChiefComplaint,
                    Diagnosis = record.Diagnosis,
                    IcdCode = record.IcdCode,
                    ClinicalNotes = record.ClinicalNotes,
                    TreatmentPlan = record.TreatmentPlan,
                    FollowUpDate = record.FollowUpDate,
                    IsConfidential = record.IsConfidential
                };

                ViewBag.RecordId = id;
                ViewBag.PatientName = record.PatientName;
                ViewBag.DoctorName = record.DoctorName;
                ViewBag.VisitDate = record.VisitDate;
                return View(dto);
            }
            catch (NotFoundException)
            {
                TempData["Error"] = "Medical record not found.";
                return RedirectToAction("Index", "Patients");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index", "Patients");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateMedicalRecordDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.RecordId = id;
                return View(dto);
            }

            try
            {
                var requestingDoctorId = 0;
                int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out requestingDoctorId);

                var updated = await _services.MedicalRecordService.UpdateMedicalRecordAsync(id, dto, requestingDoctorId);
                TempData["Success"] = "Medical record updated successfully.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (UnauthorizedAccessException)
            {
                TempData["Error"] = "You are not authorized to edit this record.";
            }
            catch (NotFoundException)
            {
                TempData["Error"] = "Medical record not found.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            ViewBag.RecordId = id;
            return View(dto);
        }

        public async Task<IActionResult> PatientRecords(int patientId, int pageIndex = 1)
        {
            try
            {
                var patient = await _services.PatientService.GetPatientByIdAsync(patientId);
                var parameters = new MedicalRecordSpecificationParameters
                {
                    PageIndex = pageIndex,
                    PageSize = 10
                };
                var result = await _services.MedicalRecordService.GetPatientMedicalRecordsAsync(patientId, parameters);

                ViewBag.PatientId = patientId;
                ViewBag.PatientName = patient.FullName;
                ViewBag.PatientStatus = patient.Status;
                ViewBag.PageIndex = pageIndex;
                return View(result);
            }
            catch (NotFoundException)
            {
                TempData["Error"] = "Patient not found.";
                return RedirectToAction("Index", "Patients");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index", "Patients");
            }
        }

        public async Task<IActionResult> DoctorRecords(int doctorId, int pageIndex = 1)
        {
            try
            {
                var doctor = await _services.DoctorService.GetDoctorByIdAsync(doctorId);
                var parameters = new MedicalRecordSpecificationParameters
                {
                    PageIndex = pageIndex,
                    PageSize = 10
                };
                var result = await _services.MedicalRecordService.GetDoctorMedicalRecordsAsync(doctorId, parameters);

                ViewBag.DoctorId = doctorId;
                ViewBag.DoctorName = doctor.FullName;
                ViewBag.PageIndex = pageIndex;
                return View(result);
            }
            catch (NotFoundException)
            {
                TempData["Error"] = "Doctor not found.";
                return RedirectToAction("Index", "Doctors");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index", "Doctors");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddVitalSign(int recordId, CreateVitalSignDto dto)
        {
            try
            {
                await _services.VitalSignService.AddVitalSignAsync(recordId, dto);
                TempData["Success"] = "Vital signs recorded successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Details), new { id = recordId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPrescription(int recordId, CreatePrescriptionDto dto)
        {
            try
            {
                await _services.PrescriptionService.AddPrescriptionAsync(recordId, dto);
                TempData["Success"] = $"Prescription for {dto.MedicationName} added successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Details), new { id = recordId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelPrescription(int recordId, int prescriptionId)
        {
            try
            {
                await _services.PrescriptionService.CancelPrescriptionAsync(recordId, prescriptionId);
                TempData["Success"] = "Prescription cancelled successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Details), new { id = recordId });
        }

        public async Task<IActionResult> DownloadPrescriptionPdf(int prescriptionId, int patientId)
        {
            try
            {
                var pdfBytes = await _services.PrescriptionService.GeneratePrescriptionPdfAsync(prescriptionId, patientId);
                return File(pdfBytes, "application/pdf", $"Prescription_{prescriptionId}.pdf");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to generate PDF: " + ex.Message;
                return RedirectToAction("Index", "Patients");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLabOrder(int recordId, CreateLabOrderDto dto)
        {
            try
            {
                await _services.LabOrderService.CreateLabOrderAsync(recordId, dto);
                TempData["Success"] = $"Lab order for {dto.TestName} created successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Details), new { id = recordId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateLabOrderStatus(int orderId, int recordId, UpdateLabOrderStatusDto dto)
        {
            try
            {
                await _services.LabOrderService.UpdateLabOrderStatusAsync(orderId, dto);
                TempData["Success"] = "Lab order status updated.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Details), new { id = recordId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLabResult(int orderId, int recordId, CreateLabResultDto dto)
        {
            try
            {
                await _services.LabOrderService.AddLabResultAsync(orderId, dto);
                TempData["Success"] = "Lab result recorded successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Details), new { id = recordId });
        }
    }
}

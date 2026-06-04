using HMS.BLL.ServicesAbstraction.Contracts;
using HMS.BLL.Shared.Dtos.PatientModule.PatientDtos;
using HMS.BLL.Shared.Parameters;
using HMS.DAL.Models.Enums.PatientEnums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HMS.PL.Controllers
{
    public class PatientsController : Controller
    {
        private readonly IServiceManager _services;

        public PatientsController(IServiceManager services)
        {
            _services = services;
        }

        // GET: /Patients
        public async Task<IActionResult> Index(string? search, string? status, int pageIndex = 1)
        {
            var parameters = new PatientSpecificationParameters
            {
                Search = search,
                Status = string.IsNullOrEmpty(status) ? null : Enum.Parse<PatientStatus>(status),
                PageIndex = pageIndex,
                PageSize = 10
            };

            var result = await _services.PatientService.GetAllPatientsAsync(parameters);

            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.StatusList = GetStatusSelectList(status);

            return View(result);
        }

        // GET: /Patients/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var patient = await _services.PatientService.GetPatientWithDetailsAsync(id);
                return View(patient);
            }
            catch
            {
                TempData["Error"] = "Patient not found.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /Patients/Create
        public IActionResult Create()
        {
            PopulateDropdowns();
            return View(new CreatePatientDto
            {
                Address = new AddressDto()
            });
        }

        // POST: /Patients/Create
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
                TempData["Success"] = $"Patient {created.FullName} registered successfully with MRN {created.MedicalRecordNumber}.";
                return RedirectToAction(nameof(Details), new { id = created.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                PopulateDropdowns();
                return View(dto);
            }
        }

        // GET: /Patients/Edit/5
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
                PopulateDropdowns(patient.Status);
                return View(dto);
            }
            catch
            {
                TempData["Error"] = "Patient not found.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Patients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdatePatientDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.PatientId = id;
                PopulateDropdowns(dto.Status?.ToString());
                return View(dto);
            }

            try
            {
                await _services.PatientService.UpdatePatientAsync(id, dto);
                TempData["Success"] = "Patient updated successfully.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.PatientId = id;
                PopulateDropdowns(dto.Status?.ToString());
                return View(dto);
            }
        }

        // POST: /Patients/Deactivate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            try
            {
                await _services.PatientService.DeactivatePatientAsync(id);
                TempData["Success"] = "Patient deactivated successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        // ── Helpers ───────────────────────────────────────────────────────

        private void PopulateDropdowns(string? selectedStatus = null)
        {
            ViewBag.GenderList = new SelectList(
                Enum.GetNames(typeof(Gender)), selectedStatus);

            ViewBag.BloodTypeList = new SelectList(
                Enum.GetNames(typeof(BloodType)));

            ViewBag.StatusList = GetStatusSelectList(selectedStatus);
        }

        private static SelectList GetStatusSelectList(string? selected = null)
        {
            var items = Enum.GetNames(typeof(PatientStatus))
                .Select(n => new SelectListItem { Value = n, Text = n })
                .ToList();
            return new SelectList(items, "Value", "Text", selected);
        }
    }
}
using HMS.BLL.ServicesAbstraction.Contracts;
using HMS.BLL.Shared.Dtos.DoctorModule.DepartmentDtos;
using Microsoft.AspNetCore.Mvc;

namespace HMS.PL.Controllers
{
    public class DepartmentsController : Controller
    {
        private readonly IServiceManager _services;

        public DepartmentsController(IServiceManager services)
        {
            _services = services;
        }

        // GET: /Departments
        public async Task<IActionResult> Index()
        {
            var departments = await _services.DepartmentService.GetAllDepartmentAsync();
            return View(departments);
        }

        // GET: /Departments/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var dept = await _services.DepartmentService.GetDepartmentByIdAsync(id);
                return View(dept);
            }
            catch
            {
                TempData["Error"] = "Department not found.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /Departments/Create
        public IActionResult Create()
        {
            return View(new CreateDepartmentDto());
        }

        // POST: /Departments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateDepartmentDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            try
            {
                var created = await _services.DepartmentService.CreateDepartmentAsync(dto);
                TempData["Success"] = $"Department '{created.Name}' created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
        }

        // GET: /Departments/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var dept = await _services.DepartmentService.GetDepartmentByIdAsync(id);
                var dto = new UpdateDepartmentDto
                {
                    Name = dept.Name,
                    Description = dept.Description,
                    PhoneExtension = dept.PhoneExtension,
                    HeadDoctorId = dept.HeadDoctorId
                };
                ViewBag.DepartmentId = id;
                ViewBag.DepartmentName = dept.Name;

                // Populate doctor list for head doctor selection
                var doctors = await _services.DoctorService.GetAllDoctorsAsync(
                    new HMS.BLL.Shared.Parameters.DoctorSpecificationParameters { PageSize = 100 });
                ViewBag.DoctorList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                    doctors.Data, "Id", "FullName", dept.HeadDoctorId);

                return View(dto);
            }
            catch
            {
                TempData["Error"] = "Department not found.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Departments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateDepartmentDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.DepartmentId = id;
                var doctors = await _services.DoctorService.GetAllDoctorsAsync(
                    new HMS.BLL.Shared.Parameters.DoctorSpecificationParameters { PageSize = 100 });
                ViewBag.DoctorList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                    doctors.Data, "Id", "FullName", dto.HeadDoctorId);
                return View(dto);
            }

            try
            {
                await _services.DepartmentService.UpadateDepartmentAsync(id, dto);
                TempData["Success"] = "Department updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.DepartmentId = id;
                return View(dto);
            }
        }
    }
}
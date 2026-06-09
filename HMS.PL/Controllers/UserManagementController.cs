using HMS.BLL.ServicesAbstraction.Contracts;
using HMS.BLL.Services.Exceptions;
using HMS.BLL.Shared.Dtos.UserManagementDtos;
using HMS.DAL.Data.DbContexts;
using HMS.DAL.Data.Identity;
using HMS.DAL.Models.DoctorModule;
using HMS.DAL.Models.Enums.DoctorEnums;
using HMS.DAL.Models.IdentityModule;
using HMS.PL.ViewModels.UserManagementModule;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using HMS.DAL.Models.Enums.PatientEnums;

namespace HMS.PL.Controllers
{
    [Authorize(Roles = "SuperAdmin,HospitalAdmin")]
    public class UserManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IServiceManager _services;
        private readonly IdentityHospitalDbContext _identityDb;
        private readonly HospitalDbContext _hospitalDb;

        public UserManagementController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IServiceManager services,
            IdentityHospitalDbContext identityDb,
            HospitalDbContext hospitalDb)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _services = services;
            _identityDb = identityDb;
            _hospitalDb = hospitalDb;
        }

        public async Task<IActionResult> Index(
            string? search, string? role, bool? locked, int pageIndex = 1)
        {
            const int pageSize = 15;
            var allowedRoles = new[] { "SuperAdmin", "HospitalAdmin", "Doctor", "Nurse", "Receptionist", "Patient" };
            if (!string.IsNullOrEmpty(role) && !allowedRoles.Contains(role))
                role = null;

            var query = _identityDb.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(u =>
                    u.FirstName.Contains(search) ||
                    u.LastName.Contains(search) ||
                    u.Email!.Contains(search));

            if (locked.HasValue)
                query = locked.Value
                    ? query.Where(u => u.LockoutEnd != null && u.LockoutEnd > DateTimeOffset.UtcNow)
                    : query.Where(u => u.LockoutEnd == null || u.LockoutEnd <= DateTimeOffset.UtcNow);

            if (!string.IsNullOrEmpty(role))
            {
                query = from u in query
                        join ur in _identityDb.UserRoles on u.Id equals ur.UserId
                        join r in _identityDb.Roles on ur.RoleId equals r.Id
                        where r.Name == role
                        select u;
            }

            var totalCount = await query.CountAsync();

            var users = await query
                .OrderByDescending(u => u.LastLoginAt)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var userIds = users.Select(u => u.Id).ToList();
            var roleLookup = await _identityDb.UserRoles
                .Where(ur => userIds.Contains(ur.UserId))
                .Join(_identityDb.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, r.Name })
                .GroupBy(x => x.UserId)
                .ToDictionaryAsync(g => g.Key, g => g.First().Name);

            var vmList = users.Select(u => new UserListViewModel
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email ?? string.Empty,
                Role = roleLookup.GetValueOrDefault(u.Id) ?? "Unknown",
                IsEmailVerified = u.IsEmailVerified,
                IsLocked = u.LockoutEnd.HasValue && u.LockoutEnd > DateTimeOffset.UtcNow,
                LastLoginAt = u.LastLoginAt,
                FailedLoginAttempts = u.FailedLoginAttempts,
                DoctorId = u.DoctorId,
                PatientId = u.PatientId
            }).ToList();

            var vm = new UserIndexPageViewModel
            {
                Users = vmList,
                SearchQuery = search,
                RoleFilter = role,
                LockedFilter = locked,
                TotalCount = totalCount,
                PageIndex = pageIndex,
                PageSize = pageSize
            };

            ViewBag.RoleSelectList = BuildRoleSelectList(role);
            return View(vm);
        }

        public async Task<IActionResult> PendingRequests(int pageIndex = 1)
        {
            const int pageSize = 15;

            var query = _identityDb.PendingAccountRequests
                .Where(r => !r.IsApproved && !r.IsRejected)
                .OrderByDescending(r => r.CreatedAt);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var vm = new PendingRequestPageViewModel
            {
                Requests = items.Select(r => new PendingRequestViewModel
                {
                    Id = r.Id,
                    FullName = r.FullName,
                    Email = r.Email,
                    RequestedRole = r.RequestedRole,
                    LicenseNumber = r.LicenseNumber,
                    Message = r.Message,
                    CreatedAt = r.CreatedAt
                }),
                TotalCount = totalCount,
                PageIndex = pageIndex,
                PageSize = pageSize
            };

            return View(vm);
        }

        public IActionResult ApproveRequest(int id)
        {
            var request = _identityDb.PendingAccountRequests.Find(id);
            if (request is null)
            {
                TempData["Error"] = "Request not found.";
                return RedirectToAction(nameof(PendingRequests));
            }

            var names = request.FullName.Split(' ', 2, StringSplitOptions.TrimEntries);
            var firstName = names[0];
            var lastName = names.Length > 1 ? names[1] : "";

            return RedirectToAction(nameof(Create), new
            {
                firstName,
                lastName,
                email = request.Email,
                role = request.RequestedRole,
                pendingRequestId = request.Id
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectRequest(int id)
        {
            var request = await _identityDb.PendingAccountRequests.FindAsync(id);
            if (request is null)
            {
                TempData["Error"] = "Request not found.";
                return RedirectToAction(nameof(PendingRequests));
            }

            request.IsRejected = true;
            request.ReviewedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);
            request.ReviewedAt = DateTimeOffset.UtcNow;
            await _identityDb.SaveChangesAsync();

            await _services.AuditService.LogAsync(
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown",
                "REQUEST_REJECTED",
                $"Rejected account request for {request.Email} ({request.RequestedRole})",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            TempData["Success"] = $"Request from <strong>{request.FullName}</strong> rejected.";
            return RedirectToAction(nameof(PendingRequests));
        }

        public async Task<IActionResult> Details(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            var roles = await _userManager.GetRolesAsync(user);

            var vm = new UserDetailsViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                Roles = roles,
                IsEmailVerified = user.IsEmailVerified,
                IsLocked = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow,
                LockoutEnd = user.LockoutEnd?.UtcDateTime,
                LastLoginAt = user.LastLoginAt,
                FailedLoginAttempts = user.FailedLoginAttempts,
                DoctorId = user.DoctorId,
                PatientId = user.PatientId,
                EmailVerificationExpiry = user.EmailVerificationExpiry,
                RefreshTokenExpiry = user.RefreshTokenExpiry
            };

            return View(vm);
        }

        public async Task<IActionResult> Create(
            string? firstName, string? lastName, string? email,
            string? role, int? pendingRequestId)
        {
            PopulateRoleDropdown(role);
            await PopulateDoctorDropdownsAsync();

            var vm = new CreateUserViewModel();
            if (!string.IsNullOrWhiteSpace(firstName)) vm.FirstName = firstName;
            if (!string.IsNullOrWhiteSpace(lastName)) vm.LastName = lastName;
            if (!string.IsNullOrWhiteSpace(email)) vm.Email = email;
            if (!string.IsNullOrWhiteSpace(role)) vm.Role = role;

            ViewBag.PendingRequestId = pendingRequestId;
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel vm, int? pendingRequestId)
        {
            ValidateRoleFields(vm);

            if (!ModelState.IsValid)
            {
                PopulateRoleDropdown(vm.Role);
                await PopulateDoctorDropdownsAsync();
                ViewBag.PendingRequestId = pendingRequestId;
                return View(vm);
            }

            var callerRole = User.IsInRole("SuperAdmin") ? "SuperAdmin"
                           : User.IsInRole("HospitalAdmin") ? "HospitalAdmin"
                           : null;

            try
            {
                int? doctorId = null;

                if (vm.Role == "Doctor")
                {
                    var deptExists = await _hospitalDb.Departments.AnyAsync(d => d.Id == vm.DepartmentId);
                    if (!deptExists)
                    {
                        ModelState.AddModelError(string.Empty, "Selected department does not exist.");
                        PopulateRoleDropdown(vm.Role);
                        await PopulateDoctorDropdownsAsync();
                        ViewBag.PendingRequestId = pendingRequestId;
                        return View(vm);
                    }

                    var duplicateEmail = await _hospitalDb.Doctors.AnyAsync(d => d.Email == vm.Email);
                    if (duplicateEmail)
                    {
                        ModelState.AddModelError(string.Empty, "A doctor with this email already exists.");
                        PopulateRoleDropdown(vm.Role);
                        await PopulateDoctorDropdownsAsync();
                        ViewBag.PendingRequestId = pendingRequestId;
                        return View(vm);
                    }

                    if (!string.IsNullOrWhiteSpace(vm.LicenseNumber))
                    {
                        var duplicateLicense = await _hospitalDb.Doctors.AnyAsync(d => d.LicenseNumber == vm.LicenseNumber);
                        if (duplicateLicense)
                        {
                            ModelState.AddModelError(string.Empty, "A doctor with this license number already exists.");
                            PopulateRoleDropdown(vm.Role);
                            await PopulateDoctorDropdownsAsync();
                            ViewBag.PendingRequestId = pendingRequestId;
                            return View(vm);
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(vm.NationalId))
                    {
                        var duplicateNationalId = await _hospitalDb.Doctors.AnyAsync(d => d.NationalId == vm.NationalId);
                        if (duplicateNationalId)
                        {
                            ModelState.AddModelError(string.Empty, "A doctor with this National ID already exists.");
                            PopulateRoleDropdown(vm.Role);
                            await PopulateDoctorDropdownsAsync();
                            ViewBag.PendingRequestId = pendingRequestId;
                            return View(vm);
                        }
                    }

                    var now = DateTime.UtcNow;
                    var doctor = new Doctor
                    {
                        FirstName = vm.FirstName,
                        LastName = vm.LastName,
                        Email = vm.Email,
                        Phone = vm.Phone ?? string.Empty,
                        DateOfBirth = vm.DateOfBirth ?? now.AddYears(-30),
                        Gender = vm.Gender ?? Gender.Male,
                        NationalId = vm.NationalId ?? string.Empty,
                        LicenseNumber = vm.LicenseNumber ?? string.Empty,
                        Specialization = vm.Specialization ?? string.Empty,
                        DepartmentId = vm.DepartmentId!.Value,
                        YearsOfExperience = vm.YearsOfExperience,
                        ConsultationFee = vm.ConsultationFee ?? 0,
                        Bio = vm.Bio,
                        Status = DoctorStatus.Active,
                        JoinDate = now
                    };

                    _hospitalDb.Doctors.Add(doctor);
                    await _hospitalDb.SaveChangesAsync();

                    doctorId = doctor.Id;
                }

                var dto = new RegisterDto
                {
                    FirstName = vm.FirstName,
                    LastName = vm.LastName,
                    Email = vm.Email,
                    Password = vm.Password,
                    Role = vm.Role,
                    DoctorId = doctorId,
                };

                var result = await _services.AuthService.RegisterAsync(dto, callerRole);

                var successMsg =
                    $"User <strong>{result.User.FullName}</strong> created successfully " +
                    $"with role <strong>{vm.Role}</strong>." +
                    (result.EmailVerificationToken is not null
                        ? " A verification email has been sent."
                        : string.Empty);

                var createdUser = await _userManager.FindByIdAsync(result.User.Id);
                if (createdUser is not null && createdUser.IsEmailVerified != vm.IsEmailVerified)
                {
                    createdUser.IsEmailVerified = vm.IsEmailVerified;
                    await _userManager.UpdateAsync(createdUser);
                }

                await _services.AuditService.LogAsync(
                    User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown",
                    "USER_CREATED",
                    $"Created user {result.User.Email} with role {vm.Role}" +
                    (doctorId.HasValue ? $" (DoctorId: {doctorId})" : ""),
                    HttpContext.Connection.RemoteIpAddress?.ToString());

                if (pendingRequestId.HasValue)
                {
                    var pending = await _identityDb.PendingAccountRequests.FindAsync(pendingRequestId.Value);
                    if (pending is not null)
                    {
                        pending.IsApproved = true;
                        pending.ReviewedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);
                        pending.ReviewedAt = DateTimeOffset.UtcNow;
                        await _identityDb.SaveChangesAsync();

                        await _services.AuditService.LogAsync(
                            User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown",
                            "REQUEST_APPROVED",
                            $"Approved account request for {pending.Email} ({pending.RequestedRole})",
                            HttpContext.Connection.RemoteIpAddress?.ToString());
                    }
                }

                if (doctorId.HasValue)
                {
                    try
                    {
                        await _services.EmailService.SendDoctorWelcomeEmailAsync(
                            vm.Email,
                            $"{vm.FirstName} {vm.LastName}",
                            doctorId.Value,
                            vm.Password);
                        successMsg += " A welcome email with login credentials has been sent.";
                    }
                    catch { }
                }

                TempData["Success"] = successMsg;
                return RedirectToAction(nameof(Details), new { id = result.User.Id });
            }
            catch (ForbiddenException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch (ConflictException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch (ValidationException ex)
            {
                foreach (var err in ex.Errors)
                    ModelState.AddModelError(string.Empty, err);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            PopulateRoleDropdown(vm.Role);
            await PopulateDoctorDropdownsAsync();
            ViewBag.PendingRequestId = pendingRequestId;
            return View(vm);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (user.Id == currentUserId)
            {
                TempData["Error"] = "You cannot edit your own account.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var roles = await _userManager.GetRolesAsync(user);
            var targetRole = roles.FirstOrDefault() ?? string.Empty;

            if (User.IsInRole("HospitalAdmin") &&
                (targetRole == "SuperAdmin" || targetRole == "HospitalAdmin"))
            {
                TempData["Error"] = "You do not have permission to edit this user.";
                return RedirectToAction(nameof(Index));
            }

            var vm = new EditUserViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                CurrentRole = targetRole,
                PhoneNumber = user.PhoneNumber,
                IsEmailVerified = user.IsEmailVerified
            };

            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, EditUserViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            var roles = await _userManager.GetRolesAsync(user);
            var targetRole = roles.FirstOrDefault() ?? string.Empty;

            if (User.IsInRole("HospitalAdmin") &&
                (targetRole == "SuperAdmin" || targetRole == "HospitalAdmin"))
            {
                TempData["Error"] = "You do not have permission to edit this user.";
                return RedirectToAction(nameof(Index));
            }

            user.FirstName = vm.FirstName;
            user.LastName = vm.LastName;
            user.PhoneNumber = vm.PhoneNumber;
            user.IsEmailVerified = vm.IsEmailVerified;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var err in result.Errors)
                    ModelState.AddModelError(string.Empty, err.Description);
                return View(vm);
            }

            await _services.AuditService.LogAsync(
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown",
                "USER_UPDATED",
                $"Updated profile of {user.Email}",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            TempData["Success"] = "User profile updated successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Lock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null) { TempData["Error"] = "User not found."; return RedirectToAction(nameof(Index)); }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (user.Id == currentUserId)
            {
                TempData["Error"] = "You cannot lock your own account.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var roles = await _userManager.GetRolesAsync(user);
            var targetRole = roles.FirstOrDefault() ?? string.Empty;

            if (User.IsInRole("HospitalAdmin") &&
                (targetRole == "SuperAdmin" || targetRole == "HospitalAdmin"))
            {
                TempData["Error"] = "You cannot lock this user.";
                return RedirectToAction(nameof(Details), new { id });
            }

            user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100);
            await _userManager.UpdateAsync(user);

            await _services.AuditService.LogAsync(
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown",
                "USER_LOCKED",
                $"Locked account of {user.Email}",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            TempData["Success"] = $"Account for {user.Email} has been locked.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Unlock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null) { TempData["Error"] = "User not found."; return RedirectToAction(nameof(Index)); }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (user.Id == currentUserId)
            {
                TempData["Error"] = "You cannot unlock your own account.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var roles = await _userManager.GetRolesAsync(user);
            var targetRole = roles.FirstOrDefault() ?? string.Empty;

            if (User.IsInRole("HospitalAdmin") &&
                (targetRole == "SuperAdmin" || targetRole == "HospitalAdmin"))
            {
                TempData["Error"] = "You cannot unlock this user.";
                return RedirectToAction(nameof(Details), new { id });
            }

            user.LockoutEnd = null;
            user.FailedLoginAttempts = 0;
            await _userManager.UpdateAsync(user);

            await _services.AuditService.LogAsync(
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown",
                "USER_UNLOCKED",
                $"Unlocked account of {user.Email}",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            TempData["Success"] = $"Account for {user.Email} has been unlocked.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ForceResetPassword(ForceResetPasswordViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Password must be at least 8 characters.";
                return RedirectToAction(nameof(Details), new { id = vm.Id });
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (vm.Id == currentUserId)
            {
                TempData["Error"] = "You cannot reset your own password.";
                return RedirectToAction(nameof(Details), new { id = vm.Id });
            }

            var user = await _userManager.FindByIdAsync(vm.Id);
            if (user is null) { TempData["Error"] = "User not found."; return RedirectToAction(nameof(Index)); }

            var roles = await _userManager.GetRolesAsync(user);
            var targetRole = roles.FirstOrDefault() ?? string.Empty;

            if (User.IsInRole("HospitalAdmin") &&
                (targetRole == "SuperAdmin" || targetRole == "HospitalAdmin"))
            {
                TempData["Error"] = "You cannot reset the password for this user.";
                return RedirectToAction(nameof(Details), new { id = vm.Id });
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, vm.NewPassword);

            if (!result.Succeeded)
            {
                TempData["Error"] = string.Join(" | ", result.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(Details), new { id = vm.Id });
            }

            user.IsEmailVerified = true;
            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;
            await _userManager.UpdateAsync(user);

            await _services.AuditService.LogAsync(
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown",
                "PASSWORD_FORCE_RESET",
                $"Force-reset password for {user.Email}",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            TempData["Success"] = $"Password for {user.Email} has been reset.";
            return RedirectToAction(nameof(Details), new { id = vm.Id });
        }

        public async Task<IActionResult> AuditLog(
            string? userId, string? action, int pageIndex = 1)
        {
            const int pageSize = 30;

            var query = _identityDb.AuditLogs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(userId))
                query = query.Where(a => a.UserId == userId);
            if (!string.IsNullOrWhiteSpace(action))
                query = query.Where(a => a.Action.Contains(action));

            var totalCount = await query.CountAsync();

            var entries = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var userIds = entries.Select(e => e.UserId).Distinct().ToList();
            var users = await _userManager.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.Email ?? u.Id);

            var vmEntries = entries.Select(e => new AuditLogEntryViewModel
            {
                Id = e.Id,
                UserId = e.UserId,
                UserEmail = users.GetValueOrDefault(e.UserId, e.UserId),
                Action = e.Action,
                Details = e.Details,
                IpAddress = e.IpAddress,
                CreatedAt = e.CreatedAt
            }).ToList();

            var vm = new AuditLogPageViewModel
            {
                Entries = vmEntries,
                UserIdFilter = userId,
                ActionFilter = action,
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };

            return View(vm);
        }

        private void PopulateRoleDropdown(string? selectedRole)
        {
            var availableRoles = User.IsInRole("SuperAdmin")
                ? new[] { "Patient", "Doctor", "Nurse", "Receptionist", "HospitalAdmin", "SuperAdmin" }
                : new[] { "Patient", "Doctor", "Nurse", "Receptionist" };

            ViewBag.RoleSelectList = new SelectList(
                availableRoles.Select(r => new { Value = r, Text = r }),
                "Value", "Text", selectedRole);
        }

        private static SelectList BuildRoleSelectList(string? selected)
        {
            var roles = new[]
            {
                new { Value = "",              Text = "All Roles" },
                new { Value = "SuperAdmin",    Text = "Super Admin" },
                new { Value = "HospitalAdmin", Text = "Hospital Admin" },
                new { Value = "Doctor",        Text = "Doctor" },
                new { Value = "Nurse",         Text = "Nurse" },
                new { Value = "Receptionist",  Text = "Receptionist" },
                new { Value = "Patient",       Text = "Patient" }
            };
            return new SelectList(roles, "Value", "Text", selected ?? string.Empty);
        }

        private void ValidateRoleFields(CreateUserViewModel vm)
        {
            if (vm.Role == "Doctor")
            {
                if (string.IsNullOrWhiteSpace(vm.Phone))
                    ModelState.AddModelError(nameof(vm.Phone), "Phone is required for Doctor registration.");
                if (!vm.DateOfBirth.HasValue)
                    ModelState.AddModelError(nameof(vm.DateOfBirth), "Date of Birth is required for Doctor registration.");
                if (!vm.Gender.HasValue)
                    ModelState.AddModelError(nameof(vm.Gender), "Gender is required for Doctor registration.");
                if (string.IsNullOrWhiteSpace(vm.Specialization))
                    ModelState.AddModelError(nameof(vm.Specialization), "Specialization is required for Doctor registration.");
                if (!vm.DepartmentId.HasValue)
                    ModelState.AddModelError(nameof(vm.DepartmentId), "Department is required for Doctor registration.");
                if (string.IsNullOrWhiteSpace(vm.NationalId) || vm.NationalId.Length != 14)
                    ModelState.AddModelError(nameof(vm.NationalId), "National ID must be exactly 14 characters.");
            }

            if (vm.Role == "Patient")
            {
                if (string.IsNullOrWhiteSpace(vm.Phone))
                    ModelState.AddModelError(nameof(vm.Phone), "Phone is required for Patient registration.");
                if (!vm.DateOfBirth.HasValue)
                    ModelState.AddModelError(nameof(vm.DateOfBirth), "Date of Birth is required for Patient registration.");
                if (!vm.Gender.HasValue)
                    ModelState.AddModelError(nameof(vm.Gender), "Gender is required for Patient registration.");
                if (string.IsNullOrWhiteSpace(vm.NationalId) || vm.NationalId.Length != 14)
                    ModelState.AddModelError(nameof(vm.NationalId), "National ID must be exactly 14 characters.");
            }
        }

        private async Task PopulateDoctorDropdownsAsync()
        {
            var departments = await _services.DepartmentService.GetAllDepartmentAsync();
            ViewBag.Departments = new SelectList(departments, "Id", "Name");
        }
    }
}

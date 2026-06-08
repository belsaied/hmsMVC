using HMS.BLL.Services.Exceptions;
using HMS.BLL.ServicesAbstraction.Contracts;
using HMS.BLL.Shared.Dtos.UserManagementDtos;
using HMS.DAL.Models.IdentityModule;
using HMS.PL.ViewModels.AuthModule;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HMS.PL.Controllers
{
    public class AuthController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuthService _authService;

        public AuthController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IAuthService authService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToDashboard();

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                await _authService.LoginAsync(new LoginDto
                {
                    Email = vm.Email,
                    Password = vm.Password
                });

                var user = await _userManager.FindByEmailAsync(vm.Email);
                if (user is not null)
                    await _signInManager.SignInAsync(user, isPersistent: vm.RememberMe);

                if (Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToDashboard();
            }
            catch (EmailNotVerifiedException)
            {
                ModelState.AddModelError(string.Empty, "Please verify your email before logging in. Check your inbox.");
            }
            catch (AccountLockedException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch (UnauthorizedException)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
            }

            return View(vm);
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToDashboard();

            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                await _authService.RegisterAsync(new RegisterDto
                {
                    FirstName = vm.FirstName,
                    LastName = vm.LastName,
                    Email = vm.Email,
                    Password = vm.Password,
                    Role = "Patient",
                }, callerRole: null);

                TempData["Success"] = "Account created successfully! Please check your email to verify your account.";
                return RedirectToAction(nameof(Login));
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

            return View(vm);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        private IActionResult RedirectToDashboard()
        {
            if (User.IsInRole("SuperAdmin") || User.IsInRole("HospitalAdmin"))
                return RedirectToAction("Index", "Dashboard");
            if (User.IsInRole("Doctor"))
                return RedirectToAction("DoctorDashboard", "Dashboard");
            return RedirectToAction("Index", "Home");
        }
    }
}

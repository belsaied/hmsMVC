using HMS.BLL.Services.Exceptions;
using HMS.BLL.ServicesAbstraction.Contracts;
using HMS.BLL.Shared.Dtos.UserManagementDtos;
using HMS.DAL.Models.IdentityModule;
using HMS.PL.ViewModels.AuthModule;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToDashboard();

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [AllowAnonymous]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

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
                {
                    await _signInManager.SignInAsync(user, isPersistent: vm.RememberMe);
                    var roles = await _userManager.GetRolesAsync(user);

                    if (Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);

                    if (roles.Contains("SuperAdmin") || roles.Contains("HospitalAdmin"))
                        return RedirectToAction("Index", "Dashboard");
                    if (roles.Contains("Doctor"))
                        return RedirectToAction("DoctorDashboard", "Dashboard");
                    return RedirectToAction("Index", "Home");
                }
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

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToDashboard();

            return View();
        }

        [AllowAnonymous]
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

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                var accessToken = HttpContext.Request.Cookies["AccessToken"] ?? string.Empty;
                try { await _authService.LogoutAsync(userId, accessToken); } catch { }
            }
            await _signInManager.SignOutAsync();
            Response.Cookies.Delete("AccessToken");
            return RedirectToAction("Login", "Auth");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogoutPost()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                var accessToken = HttpContext.Request.Cookies["AccessToken"] ?? string.Empty;
                try { await _authService.LogoutAsync(userId, accessToken); } catch { }
            }
            await _signInManager.SignOutAsync();
            Response.Cookies.Delete("AccessToken");
            return RedirectToAction("Login", "Auth");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [AllowAnonymous]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            await _authService.ForgotPasswordAsync(vm.Email);
            TempData["Success"] = "If that email is registered, a reset link has been sent.";
            return RedirectToAction(nameof(Login));
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return RedirectToAction(nameof(Login));

            return View(new ResetPasswordViewModel { Token = token });
        }

        [AllowAnonymous]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            try
            {
                await _authService.ResetPasswordAsync(new ResetPasswordDto
                {
                    Token = vm.Token,
                    NewPassword = vm.NewPassword
                });
                TempData["Success"] = "Your password has been reset. Please sign in.";
                return RedirectToAction(nameof(Login));
            }
            catch (UnauthorizedException)
            {
                ModelState.AddModelError(string.Empty, "This reset link is invalid or has expired.");
            }
            catch (ValidationException ex)
            {
                foreach (var err in ex.Errors)
                    ModelState.AddModelError(string.Empty, err);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "An unexpected error occurred.");
            }

            return View(vm);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> VerifyEmail(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                TempData["Error"] = "Invalid verification link.";
                return RedirectToAction(nameof(Login));
            }
            try
            {
                await _authService.VerifyEmailAsync(token);
                TempData["Success"] = "Email verified! You can now sign in.";
            }
            catch (UnauthorizedException)
            {
                TempData["Error"] = "Verification link is invalid or has expired.";
            }
            return RedirectToAction(nameof(Login));
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

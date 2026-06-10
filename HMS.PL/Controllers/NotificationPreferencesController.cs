using HMS.BLL.ServicesAbstraction.Contracts;
using HMS.BLL.Shared.Dtos.NotificationDtos.Requests;
using HMS.PL.ViewModels.NotificationModule;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HMS.PL.Controllers
{
    [Authorize]
    public class NotificationPreferencesController(IServiceManager _serviceManager) : Controller
    {
        private string CurrentUserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        public async Task<IActionResult> Index()
        {
            var prefs = await _serviceManager.NotificationPreferenceService
                .GetPreferencesAsync(CurrentUserId);

            var viewModel = new NotificationPreferenceViewModel
            {
                Preferences = prefs
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update([FromBody] UpdatePreferenceRequest request)
        {
            var result = await _serviceManager.NotificationPreferenceService
                .UpdatePreferenceAsync(CurrentUserId, request);
            return Json(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reset()
        {
            await _serviceManager.NotificationPreferenceService
                .ResetPreferencesToDefaultAsync(CurrentUserId);
            TempData["Success"] = "Notification preferences have been reset to default.";
            return RedirectToAction(nameof(Index));
        }
    }
}

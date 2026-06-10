using HMS.BLL.ServicesAbstraction.Contracts;
using HMS.BLL.Shared.Dtos.NotificationDtos.Requests;
using HMS.PL.ViewModels.NotificationModule;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HMS.PL.Controllers
{
    [Authorize]
    public class NotificationsController(IServiceManager _serviceManager) : Controller
    {
        private string CurrentUserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        public async Task<IActionResult> Index([FromQuery] NotificationLogFilter filter)
        {
            var result = await _serviceManager.NotificationLogService
                .GetNotificationsByUserAsync(CurrentUserId, filter);

            var viewModel = new NotificationLogViewModel
            {
                PaginatedResult = result,
                Filter = filter
            };

            return View(viewModel);
        }

        [Authorize(Roles = "SuperAdmin,HospitalAdmin")]
        public async Task<IActionResult> AdminLog([FromQuery] NotificationLogFilter filter)
        {
            var result = await _serviceManager.AdminNotificationLogService
                .GetAllNotificationsAsync(filter);

            var viewModel = new AdminNotificationLogViewModel
            {
                PaginatedResult = result,
                Filter = filter
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> UnreadCount()
        {
            var count = await _serviceManager.NotificationLogService
                .GetUnreadPushCountAsync(CurrentUserId);
            return Json(count);
        }

        [HttpGet]
        public async Task<IActionResult> UnreadPush()
        {
            var result = await _serviceManager.NotificationLogService
                .GetUnreadPushNotificationsAsync(CurrentUserId);
            return Json(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkRead(Guid id)
        {
            await _serviceManager.NotificationLogService.MarkPushNotificationReadAsync(id);
            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllRead()
        {
            await _serviceManager.NotificationLogService
                .MarkAllPushNotificationsReadAsync(CurrentUserId);
            return Ok();
        }
    }
}

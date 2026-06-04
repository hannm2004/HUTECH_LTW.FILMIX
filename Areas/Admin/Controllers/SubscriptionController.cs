using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using untitled1.Models.Entities;
using untitled1.Services;

namespace untitled1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SubscriptionController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogService _logService;
        private const int PageSize = 15;

        public SubscriptionController(
            IAdminService adminService,
            UserManager<ApplicationUser> userManager,
            ILogService logService)
        {
            _adminService = adminService;
            _userManager = userManager;
            _logService = logService;
        }

        // GET: Admin/Subscription
        public async Task<IActionResult> Index(string? search, string filter = "all", int page = 1)
        {
            var vm = await _adminService.GetSubscriptionsAsync(search, filter, page, PageSize);
            return View(vm);
        }

        // POST: Admin/Subscription/Deactivate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            var ok = await _adminService.DeactivateSubscriptionAsync(id);
            if (ok)
            {
                var admin = await _userManager.GetUserAsync(User);
                await _logService.LogAsync(
                    admin?.Id, 
                    admin?.Email, 
                    "Deactivate Subscription", 
                    $"Hủy kích hoạt gói đăng ký #{id}", 
                    HttpContext.Connection.RemoteIpAddress?.ToString()
                );
            }

            TempData[ok ? "Success" : "Error"] = ok
                ? $"Đã hủy kích hoạt gói đăng ký #{id}."
                : $"Không tìm thấy gói đăng ký #{id}.";
            return RedirectToAction(nameof(Index));
        }
    }
}

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using untitled1.Services;

namespace untitled1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SubscriptionController : Controller
    {
        private readonly IAdminService _adminService;
        private const int PageSize = 15;

        public SubscriptionController(IAdminService adminService)
        {
            _adminService = adminService;
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
            TempData[ok ? "Success" : "Error"] = ok
                ? $"Đã hủy kích hoạt gói đăng ký #{id}."
                : $"Không tìm thấy gói đăng ký #{id}.";
            return RedirectToAction(nameof(Index));
        }
    }
}

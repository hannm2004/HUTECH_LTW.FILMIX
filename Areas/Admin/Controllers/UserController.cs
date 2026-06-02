using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using untitled1.Services;

namespace untitled1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly IAdminService _adminService;
        private const int PageSize = 15;

        public UserController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        // GET: Admin/User
        public async Task<IActionResult> Index(string? search, string filter = "all", int page = 1)
        {
            var vm = await _adminService.GetUsersAsync(search, filter, page, PageSize);
            return View(vm);
        }

        // GET: Admin/User/Detail/id
        public async Task<IActionResult> Detail(string id)
        {
            var vm = await _adminService.GetUserDetailAsync(id);
            if (vm == null) return NotFound();
            return View(vm);
        }

        // POST: Admin/User/SetAdmin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetAdmin(string userId, bool grantAdmin)
        {
            var ok = await _adminService.SetAdminRoleAsync(userId, grantAdmin);
            TempData[ok ? "Success" : "Error"] = ok
                ? (grantAdmin ? "Đã cấp quyền Admin cho người dùng." : "Đã thu hồi quyền Admin.")
                : "Không tìm thấy người dùng.";
            return RedirectToAction(nameof(Detail), new { id = userId });
        }

        // POST: Admin/User/TogglePremium
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePremium(string userId, bool activate, int days = 30)
        {
            var ok = await _adminService.TogglePremiumAsync(userId, activate, days);
            TempData[ok ? "Success" : "Error"] = ok
                ? (activate ? $"Đã kích hoạt Premium {days} ngày cho người dùng." : "Đã thu hồi Premium.")
                : "Không tìm thấy người dùng.";
            return RedirectToAction(nameof(Detail), new { id = userId });
        }
    }
}

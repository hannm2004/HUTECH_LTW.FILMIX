using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using untitled1.Services;

namespace untitled1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AnalyticsController : Controller
    {
        private readonly IAdminService _adminService;

        public AnalyticsController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        // GET: Admin/Analytics
        public async Task<IActionResult> Index()
        {
            var vm = await _adminService.GetAnalyticsAsync();
            return View(vm);
        }
    }
}

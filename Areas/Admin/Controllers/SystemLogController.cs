using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using untitled1.Services;

namespace untitled1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SystemLogController : Controller
    {
        private readonly ILogService _logService;
        private const int PageSize = 20;

        public SystemLogController(ILogService logService)
        {
            _logService = logService;
        }

        // GET: Admin/SystemLog
        public async Task<IActionResult> Index(string? search, string actionType = "all", int page = 1)
        {
            var vm = await _logService.GetLogsAsync(search, actionType, page, PageSize);
            return View(vm);
        }

        // GET: Admin/SystemLog/Detail/5
        public async Task<IActionResult> Detail(int id)
        {
            var log = await _logService.GetLogDetailAsync(id);
            if (log == null) return NotFound();
            return View(log);
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using untitled1.Data;
using System.Threading.Tasks;
using System.Linq;

namespace untitled1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalMovies = await _context.Movies.CountAsync();
            ViewBag.TotalTVSeries = await _context.Movies.CountAsync(m => m.IsTVSeries);
            ViewBag.TotalFilms = await _context.Movies.CountAsync(m => !m.IsTVSeries);
            ViewBag.TotalUsers = await _context.Users.CountAsync();

            ViewBag.RecentMovies = await _context.Movies
                .OrderByDescending(m => m.Id)
                .Take(5)
                .ToListAsync();

            return View();
        }
    }
}

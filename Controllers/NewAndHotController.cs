using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using untitled1.Data;

namespace untitled1.Controllers
{
    public class NewAndHotController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NewAndHotController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var trendingMovies = await _context.Movies
                .Where(m => m.IsTrending)
                .ToListAsync();
            return View(trendingMovies);
        }
    }
}

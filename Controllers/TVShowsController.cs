using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using untitled1.Data;
using System.Threading.Tasks;

namespace untitled1.Controllers
{
    public class TVShowsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TVShowsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var tvShows = await _context.Movies.Where(m => m.IsTVSeries).ToListAsync();
            return View(tvShows);
        }
    }
}

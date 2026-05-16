using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using untitled1.Data;
using System.Threading.Tasks;
using System.Linq;

namespace untitled1.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MoviesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Filter only Single Movies (IsTVSeries = false)
            var movies = await _context.Movies.Where(m => !m.IsTVSeries).ToListAsync();
            return View(movies);
        }
    }
}

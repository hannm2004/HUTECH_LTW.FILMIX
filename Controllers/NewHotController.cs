using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using untitled1.Data;
using System.Threading.Tasks;
using System.Linq;

namespace untitled1.Controllers
{
    public class NewHotController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NewHotController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Query all movies and tv shows, ordered by release year descending to show the newest items first
            var newHotItems = await _context.Movies
                .OrderByDescending(m => m.Year)
                .ToListAsync();
            return View(newHotItems);
        }
    }
}

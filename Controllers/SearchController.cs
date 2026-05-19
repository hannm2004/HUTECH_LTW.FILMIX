using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using untitled1.Data;
using System.Linq;
using System.Threading.Tasks;

namespace untitled1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SearchController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return Ok(new object[] { });
            }

            var query = q.ToLower();

            var movies = await _context.Movies
                .Where(m => (!string.IsNullOrEmpty(m.Title) && m.Title.ToLower().Contains(query)) || 
                            (!string.IsNullOrEmpty(m.Director) && m.Director.ToLower().Contains(query)) || 
                            (!string.IsNullOrEmpty(m.Cast) && m.Cast.ToLower().Contains(query)) || 
                            (!string.IsNullOrEmpty(m.Genre) && m.Genre.ToLower().Contains(query)))
                .Select(m => new 
                {
                    m.Id,
                    m.Title,
                    m.ImageUrl,
                    m.Year,
                    m.Genre
                })
                .Take(6)
                .ToListAsync();

            return Ok(movies);
        }
    }
}

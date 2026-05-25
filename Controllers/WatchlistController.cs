using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using untitled1.Data;
using System.Threading.Tasks;
using System.Linq;

namespace untitled1.Controllers
{
    public class WatchlistController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WatchlistController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Trang "Danh Sách Của Tôi" — đọc IDs từ query string (client gửi lên),
        /// tải phim từ DB, trả về View để render.
        /// </summary>
        public async Task<IActionResult> Index(string? ids)
        {
            // ids là comma-separated list của movie IDs, VD: "1,3,7"
            if (string.IsNullOrWhiteSpace(ids))
                return View(Enumerable.Empty<untitled1.Models.Entities.Movie>());

            var idList = ids.Split(',')
                .Select(x => int.TryParse(x.Trim(), out var n) ? n : 0)
                .Where(n => n > 0)
                .Distinct()
                .ToList();

            if (!idList.Any())
                return View(Enumerable.Empty<untitled1.Models.Entities.Movie>());

            var movies = await _context.Movies
                .Where(m => idList.Contains(m.Id))
                .ToListAsync();

            // Preserve the original ordering from the watchlist
            var ordered = idList
                .Select(id => movies.FirstOrDefault(m => m.Id == id))
                .Where(m => m != null)
                .ToList();

            return View(ordered);
        }

        /// <summary>
        /// API endpoint — trả JSON cho movie IDs, dùng bởi JS khi localStorage thay đổi.
        /// GET /api/watchlist?ids=1,2,3
        /// </summary>
        [Route("api/watchlist")]
        [HttpGet]
        public async Task<IActionResult> ApiGet(string? ids)
        {
            if (string.IsNullOrWhiteSpace(ids))
                return Ok(new object[] { });

            var idList = ids.Split(',')
                .Select(x => int.TryParse(x.Trim(), out var n) ? n : 0)
                .Where(n => n > 0)
                .Distinct()
                .ToList();

            var movies = await _context.Movies
                .Where(m => idList.Contains(m.Id))
                .Select(m => new { m.Id, m.Title, m.ImageUrl, m.Year, m.Genre, m.IsTVSeries })
                .ToListAsync();

            return Ok(movies);
        }
    }
}

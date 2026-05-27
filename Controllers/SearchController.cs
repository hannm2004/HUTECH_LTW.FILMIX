using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using untitled1.Data;
using untitled1.Models.Entities;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace untitled1.Controllers
{
    public class SearchController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SearchController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET /Search?q=batman
        public async Task<IActionResult> Index(string? q, string? type, int? year)
        {
            ViewBag.Query = q ?? string.Empty;
            ViewBag.Type  = type ?? "all";
            ViewBag.Year  = year;

            if (string.IsNullOrWhiteSpace(q))
                return View(new List<Movie>());

            var trimmed = q.Trim().ToLower();

            var query = _context.Movies
                .Include(m => m.MovieCategories)
                    .ThenInclude(mc => mc.Category)
                .AsQueryable();

            // --- Filters ---
            if (type == "movie")   query = query.Where(m => !m.IsTVSeries);
            if (type == "series")  query = query.Where(m => m.IsTVSeries);
            if (year.HasValue)     query = query.Where(m => m.Year == year.Value);

            // --- Full-text search (EF Core LIKE) ---
            var results = await query
                .Where(m =>
                    EF.Functions.Like(m.Title.ToLower(),       $"%{trimmed}%") ||
                    EF.Functions.Like(m.Genre.ToLower(),       $"%{trimmed}%") ||
                    EF.Functions.Like(m.Director.ToLower(),    $"%{trimmed}%") ||
                    EF.Functions.Like(m.Cast.ToLower(),        $"%{trimmed}%") ||
                    EF.Functions.Like(m.Description.ToLower(), $"%{trimmed}%")
                )
                // Rank: title match first, then by year desc
                .OrderByDescending(m => m.Title.ToLower().Contains(trimmed))
                .ThenByDescending(m => m.Year)
                .ToListAsync();

            return View(results);
        }

        // GET /api/search/suggest?q=bat  — JSON autocomplete
        [HttpGet("/api/search/suggest")]
        public async Task<IActionResult> Suggest(string? q)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
                return Ok(new object[] { });

            var trimmed = q.Trim().ToLower();

            var suggestions = await _context.Movies
                .Where(m => EF.Functions.Like(m.Title.ToLower(), $"%{trimmed}%"))
                .OrderByDescending(m => m.Title.ToLower().StartsWith(trimmed))
                .ThenByDescending(m => m.Year)
                .Take(8)
                .Select(m => new { m.Id, m.Title, m.ImageUrl, m.Year, m.Genre, m.IsTVSeries })
                .ToListAsync();

            return Ok(suggestions);
        }
    }
}

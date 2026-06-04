using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using untitled1.Data;
using untitled1.Models.Entities;

namespace untitled1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> _userManager;
        private readonly untitled1.Services.ILogService _logService;
        private const int PageSize = 10;

        public ProductController(
            ApplicationDbContext context, 
            Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userManager,
            untitled1.Services.ILogService logService)
        {
            _context = context;
            _userManager = userManager;
            _logService = logService;
        }

        // GET: Admin/Product
        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            var query = _context.Movies
                .Include(m => m.MovieCategories)
                    .ThenInclude(mc => mc.Category)
                .AsSplitQuery()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(m => m.Title.Contains(search));

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

            var movies = await query
                .OrderBy(m => m.Id)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.Search     = search;
            ViewBag.Page       = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;

            return View(movies);
        }

        // GET: Admin/Product/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View();
        }

        // POST: Admin/Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Movie movie, int[] selectedCategories)
        {
            if (ModelState.IsValid)
            {
                _context.Movies.Add(movie);
                await _context.SaveChangesAsync();

                foreach (var catId in selectedCategories)
                    _context.MovieCategories.Add(new MovieCategory { MovieId = movie.Id, CategoryId = catId });
                await _context.SaveChangesAsync();

                var admin = await _userManager.GetUserAsync(User);
                await _logService.LogAsync(admin?.Id, admin?.Email, "Add Movie", $"Thêm phim mới: \"{movie.Title}\" (ID: {movie.Id})", HttpContext.Connection.RemoteIpAddress?.ToString());

                TempData["Success"] = $"Đã thêm phim \"{movie.Title}\" thành công.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(movie);
        }

        // GET: Admin/Product/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var movie = await _context.Movies
                .Include(m => m.MovieCategories)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null) return NotFound();

            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.SelectedCategories = movie.MovieCategories.Select(mc => mc.CategoryId).ToList();
            return View(movie);
        }

        // POST: Admin/Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Movie movieData, int[] selectedCategories)
        {
            if (id != movieData.Id) return BadRequest();

            if (ModelState.IsValid)
            {
                var movie = await _context.Movies.FindAsync(id);
                if (movie == null) return NotFound();

                movie.Title       = movieData.Title;
                movie.ImageUrl    = movieData.ImageUrl;
                movie.Year        = movieData.Year;
                movie.Genre       = movieData.Genre;
                movie.IsTVSeries  = movieData.IsTVSeries;
                movie.Director    = movieData.Director    ?? string.Empty;
                movie.Cast        = movieData.Cast        ?? string.Empty;
                movie.Description = movieData.Description ?? string.Empty;
                movie.TrailerUrl  = movieData.TrailerUrl  ?? string.Empty;

                var existing = _context.MovieCategories.Where(mc => mc.MovieId == id);
                _context.MovieCategories.RemoveRange(existing);

                foreach (var catId in selectedCategories)
                    _context.MovieCategories.Add(new MovieCategory { MovieId = id, CategoryId = catId });

                await _context.SaveChangesAsync();

                var admin = await _userManager.GetUserAsync(User);
                await _logService.LogAsync(admin?.Id, admin?.Email, "Edit Movie", $"Sửa thông tin phim: \"{movie.Title}\" (ID: {movie.Id})", HttpContext.Connection.RemoteIpAddress?.ToString());

                TempData["Success"] = $"Đã cập nhật phim \"{movie.Title}\" thành công.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.SelectedCategories = selectedCategories.ToList();
            return View(movieData);
        }

        // GET: Admin/Product/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var movie = await _context.Movies
                .Include(m => m.MovieCategories)
                    .ThenInclude(mc => mc.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null) return NotFound();
            return View(movie);
        }

        // POST: Admin/Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movies
                .Include(m => m.MovieCategories)
                .Include(m => m.Episodes)
                .Include(m => m.MovieImages)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie != null)
            {
                var title = movie.Title;
                _context.Movies.Remove(movie);
                await _context.SaveChangesAsync();

                var admin = await _userManager.GetUserAsync(User);
                await _logService.LogAsync(admin?.Id, admin?.Email, "Delete Movie", $"Xóa phim: \"{title}\" (ID: {id})", HttpContext.Connection.RemoteIpAddress?.ToString());

                TempData["Success"] = $"Đã xóa phim \"{title}\" thành công.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}

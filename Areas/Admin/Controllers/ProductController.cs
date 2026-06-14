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
        private readonly IWebHostEnvironment _webHostEnvironment;
        private const int PageSize = 10;

        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".webp"
        };
        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

        public ProductController(
            ApplicationDbContext context, 
            Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userManager,
            untitled1.Services.ILogService logService,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _logService = logService;
            _webHostEnvironment = webHostEnvironment;
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
        public async Task<IActionResult> Create(Movie movie, int[] selectedCategories, IFormFile? posterFile)
        {
            // L-01: Handle poster file upload
            var uploadedPath = await HandlePosterUploadAsync(posterFile);
            if (uploadedPath == "INVALID_FILE")
            {
                ModelState.AddModelError("posterFile", "File không hợp lệ. Chỉ chấp nhận .jpg, .jpeg, .png, .webp và kích thước tối đa 5MB.");
            }
            else if (uploadedPath != null)
            {
                movie.ImageUrl = uploadedPath;
            }
            else if (string.IsNullOrWhiteSpace(movie.ImageUrl))
            {
                movie.ImageUrl = "/images/posters/default.jpg";
            }

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
        public async Task<IActionResult> Edit(int id, Movie movieData, int[] selectedCategories, IFormFile? posterFile)
        {
            if (id != movieData.Id) return BadRequest();

            // L-01: Handle poster file upload
            var uploadedPath = await HandlePosterUploadAsync(posterFile);
            if (uploadedPath == "INVALID_FILE")
            {
                ModelState.AddModelError("posterFile", "File không hợp lệ. Chỉ chấp nhận .jpg, .jpeg, .png, .webp và kích thước tối đa 5MB.");
            }

            if (ModelState.IsValid)
            {
                var movie = await _context.Movies.FindAsync(id);
                if (movie == null) return NotFound();

                // L-01: Delete old locally-uploaded poster if a new file is uploaded
                if (uploadedPath != null && uploadedPath != "INVALID_FILE")
                {
                    if (!string.IsNullOrEmpty(movie.ImageUrl) && movie.ImageUrl.StartsWith("/images/posters/"))
                    {
                        var oldPath = Path.Combine(_webHostEnvironment.WebRootPath, movie.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                        if (System.IO.File.Exists(oldPath))
                            System.IO.File.Delete(oldPath);
                    }
                    movie.ImageUrl = uploadedPath;
                }
                else if (uploadedPath == null && !string.IsNullOrWhiteSpace(movieData.ImageUrl))
                {
                    movie.ImageUrl = movieData.ImageUrl;
                }

                movie.Title       = movieData.Title;
                movie.Year        = movieData.Year;
                movie.Genre       = movieData.Genre;
                movie.IsTVSeries  = movieData.IsTVSeries;
                movie.Director    = movieData.Director    ?? string.Empty;
                movie.Cast        = movieData.Cast        ?? string.Empty;
                movie.Description = movieData.Description ?? string.Empty;
                movie.TrailerUrl  = movieData.TrailerUrl  ?? string.Empty;

                var existingCats = _context.MovieCategories.Where(mc => mc.MovieId == id);
                _context.MovieCategories.RemoveRange(existingCats);

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
                // L-01: Delete uploaded poster file if it's locally stored
                if (!string.IsNullOrEmpty(movie.ImageUrl) && movie.ImageUrl.StartsWith("/images/posters/"))
                {
                    var posterPath = Path.Combine(_webHostEnvironment.WebRootPath, movie.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(posterPath))
                        System.IO.File.Delete(posterPath);
                }

                var title = movie.Title;
                _context.Movies.Remove(movie);
                await _context.SaveChangesAsync();

                var admin = await _userManager.GetUserAsync(User);
                await _logService.LogAsync(admin?.Id, admin?.Email, "Delete Movie", $"Xóa phim: \"{title}\" (ID: {id})", HttpContext.Connection.RemoteIpAddress?.ToString());

                TempData["Success"] = $"Đã xóa phim \"{title}\" thành công.";
            }
            return RedirectToAction(nameof(Index));
        }

        // L-01: Helper — validates and saves an uploaded poster file.
        // Returns: file path on success, null if no file, "INVALID_FILE" on failure.
        private async Task<string?> HandlePosterUploadAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;

            var extension = Path.GetExtension(file.FileName);

            // Security: reject disallowed extensions including double-extension attacks
            if (!AllowedExtensions.Contains(extension))
                return "INVALID_FILE";

            // Security: reject files exceeding max size
            if (file.Length > MaxFileSizeBytes)
                return "INVALID_FILE";

            // Security: verify actual content type matches extension
            var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowedMimeTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
                return "INVALID_FILE";

            var postersDir = Path.Combine(_webHostEnvironment.WebRootPath, "images", "posters");
            Directory.CreateDirectory(postersDir);

            // Generate a unique filename to prevent path traversal and overwrite attacks
            var uniqueName = $"{Guid.NewGuid()}{extension.ToLowerInvariant()}";
            var filePath = Path.Combine(postersDir, uniqueName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/images/posters/{uniqueName}";
        }
    }
}

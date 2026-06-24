using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using untitled1.Data;
using untitled1.Models.Entities;

namespace untitled1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> _userManager;
        private readonly untitled1.Services.ILogService _logService;

        public CategoryController(
            ApplicationDbContext context,
            Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userManager,
            untitled1.Services.ILogService logService)
        {
            _context = context;
            _userManager = userManager;
            _logService = logService;
        }

        // GET: Admin/Category
        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.Categories
                .Include(c => c.MovieCategories)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(c => c.Name.Contains(search));

            var categories = await query
                .OrderBy(c => c.Id)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.TotalCount = categories.Count;
            return View(categories);
        }

        // GET: Admin/Category/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            await ValidateCategoryAsync(category);

            if (ModelState.IsValid)
            {
                category.Name = category.Name.Trim();
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                var admin = await _userManager.GetUserAsync(User);
                await _logService.LogAsync(admin?.Id, admin?.Email, "Add Category",
                    $"Thêm danh mục: \"{category.Name}\" (ID: {category.Id})",
                    HttpContext.Connection.RemoteIpAddress?.ToString());

                TempData["Success"] = $"Đã thêm danh mục \"{category.Name}\" thành công.";
                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        // GET: Admin/Category/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (category == null) return NotFound();
            return View(category);
        }

        // POST: Admin/Category/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category categoryData)
        {
            if (id != categoryData.Id) return BadRequest();

            await ValidateCategoryAsync(categoryData);

            if (ModelState.IsValid)
            {
                var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
                if (category == null) return NotFound();

                category.Name = categoryData.Name.Trim();
                await _context.SaveChangesAsync();

                var admin = await _userManager.GetUserAsync(User);
                await _logService.LogAsync(admin?.Id, admin?.Email, "Edit Category",
                    $"Sửa danh mục: \"{category.Name}\" (ID: {category.Id})",
                    HttpContext.Connection.RemoteIpAddress?.ToString());

                TempData["Success"] = $"Đã cập nhật danh mục \"{category.Name}\" thành công.";
                return RedirectToAction(nameof(Index));
            }

            return View(categoryData);
        }

        // GET: Admin/Category/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories
                .Include(c => c.MovieCategories)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null) return NotFound();
            return View(category);
        }

        // POST: Admin/Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories
                .Include(c => c.MovieCategories)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category != null)
            {
                var name = category.Name;
                // Gỡ liên kết phim-danh mục trước (phim chỉ mất nhãn này, không bị xóa)
                if (category.MovieCategories.Count > 0)
                    _context.MovieCategories.RemoveRange(category.MovieCategories);

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                var admin = await _userManager.GetUserAsync(User);
                await _logService.LogAsync(admin?.Id, admin?.Email, "Delete Category",
                    $"Xóa danh mục: \"{name}\" (ID: {id})",
                    HttpContext.Connection.RemoteIpAddress?.ToString());

                TempData["Success"] = $"Đã xóa danh mục \"{name}\" thành công.";
            }
            return RedirectToAction(nameof(Index));
        }

        // Helper — kiểm tra tên trống và trùng (không phân biệt hoa/thường), bỏ qua chính nó khi sửa.
        private async Task ValidateCategoryAsync(Category category)
        {
            if (string.IsNullOrWhiteSpace(category.Name))
            {
                ModelState.AddModelError(nameof(Category.Name), "Tên danh mục là bắt buộc.");
                return;
            }

            var name = category.Name.Trim();
            var duplicated = await _context.Categories
                .AnyAsync(c => c.Id != category.Id && c.Name.ToLower() == name.ToLower());
            if (duplicated)
                ModelState.AddModelError(nameof(Category.Name), $"Danh mục \"{name}\" đã tồn tại.");
        }
    }
}

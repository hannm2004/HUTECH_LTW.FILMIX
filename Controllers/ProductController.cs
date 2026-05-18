using Microsoft.AspNetCore.Mvc;
using untitled1.Models.Entities;
using untitled1.Data;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace untitled1.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // List Page
        public async Task<IActionResult> List(int? categoryId)
        {
            var categories = await _context.Categories.ToListAsync();
            var movies = categoryId.HasValue 
                ? await _context.Movies
                    .Include(m => m.MovieCategories)
                    .Where(m => m.MovieCategories.Any(mc => mc.CategoryId == categoryId.Value))
                    .ToListAsync() 
                : await _context.Movies.ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.SelectedCategoryId = categoryId;

            return View(movies);
        }

        // Detail Page (User called it tv-shows/product-detail)
        public async Task<IActionResult> Detail(int id)
        {
            var movie = await _context.Movies
                .Include(m => m.Episodes)
                .Include(m => m.MovieCategories)
                    .ThenInclude(mc => mc.Category)
                .AsSplitQuery()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null) return NotFound();

            return View(movie);
        }
    }
}

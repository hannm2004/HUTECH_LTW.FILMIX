using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using untitled1.Data;
using untitled1.Models.Entities;

namespace untitled1.ViewComponents
{
    public class HeroBannerViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public HeroBannerViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Lấy top 5 phim có rating cao nhất để làm Hero Banner
            var topMovies = await _context.Movies
                .Where(m => !string.IsNullOrEmpty(m.ImageUrl) && m.Rating > 0)
                .OrderByDescending(m => m.Rating)
                .Take(5)
                .ToListAsync();

            // Nếu không có phim có rating, fallback về tất cả phim có ảnh
            if (!topMovies.Any())
            {
                topMovies = await _context.Movies
                    .Where(m => !string.IsNullOrEmpty(m.ImageUrl))
                    .Take(5)
                    .ToListAsync();
            }

            // Rotate theo giờ để banner tự động đổi phim mỗi giờ (không random mỗi request)
            Movie? featuredMovie = null;
            if (topMovies.Any())
            {
                var hourSlot = DateTime.Now.Hour % topMovies.Count;
                featuredMovie = topMovies[hourSlot];
            }

            return View("Default", featuredMovie);
        }
    }
}

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
            // Lấy danh sách phim có hình ảnh để làm Hero Banner
            var movies = await _context.Movies
                .Where(m => !string.IsNullOrEmpty(m.ImageUrl))
                .ToListAsync();

            Movie? randomMovie = null;

            if (movies.Any())
            {
                // Chọn ngẫu nhiên 1 phim
                var random = new Random();
                randomMovie = movies[random.Next(movies.Count)];
            }

            return View("Default", randomMovie);
        }
    }
}

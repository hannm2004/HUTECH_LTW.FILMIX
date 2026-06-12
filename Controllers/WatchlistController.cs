using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using untitled1.Data;
using untitled1.Models.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace untitled1.Controllers
{
    public class WatchlistController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public WatchlistController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Trang "Danh Sách Của Tôi". Với người đã đăng nhập, danh sách được nạp từ DB
        /// (client hydrate localStorage qua /api/watchlist/ids rồi render). Khách dùng localStorage cục bộ.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var uid = _userManager.GetUserId(User)!;
                var movies = await _context.WatchlistItems
                    .Where(w => w.UserId == uid)
                    .OrderByDescending(w => w.AddedAt)
                    .Select(w => w.Movie)
                    .ToListAsync();
                return View(movies);
            }
            return View(Enumerable.Empty<Movie>());
        }

        /// <summary>
        /// GET /api/watchlist?ids=1,2,3 — trả chi tiết phim theo danh sách ID (dùng để render thẻ).
        /// </summary>
        [Route("api/watchlist")]
        [HttpGet]
        public async Task<IActionResult> ApiGet(string? ids)
        {
            if (string.IsNullOrWhiteSpace(ids))
                return Ok(new object[] { });

            var idList = ParseIds(ids);
            if (idList.Count == 0)
                return Ok(new object[] { });

            var movies = await _context.Movies
                .Where(m => idList.Contains(m.Id))
                .Select(m => new { m.Id, m.Title, m.ImageUrl, m.Year, m.Genre, m.IsTVSeries })
                .ToListAsync();

            return Ok(movies);
        }

        /// <summary>
        /// GET /api/watchlist/ids — danh sách ID phim trong watchlist của user hiện tại (mới nhất trước).
        /// Khách chưa đăng nhập → mảng rỗng.
        /// </summary>
        [Route("api/watchlist/ids")]
        [HttpGet]
        public async Task<IActionResult> Ids()
        {
            if (User.Identity?.IsAuthenticated != true)
                return Ok(new int[] { });

            var uid = _userManager.GetUserId(User)!;
            var ids = await _context.WatchlistItems
                .Where(w => w.UserId == uid)
                .OrderByDescending(w => w.AddedAt)
                .Select(w => w.MovieId)
                .ToListAsync();

            return Ok(ids);
        }

        /// <summary>
        /// POST /api/watchlist/sync — thay toàn bộ watchlist của user bằng danh sách ID gửi lên.
        /// Body: JSON mảng số nguyên, ví dụ [1,5,7]. Chỉ cho user đã đăng nhập.
        /// </summary>
        [Route("api/watchlist/sync")]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Sync([FromBody] List<int>? movieIds)
        {
            var uid = _userManager.GetUserId(User)!;
            var desired = (movieIds ?? new List<int>()).Where(id => id > 0).Distinct().ToList();

            // Chỉ giữ các ID phim có thật trong DB
            if (desired.Count > 0)
            {
                desired = await _context.Movies
                    .Where(m => desired.Contains(m.Id))
                    .Select(m => m.Id)
                    .ToListAsync();
            }

            var existing = await _context.WatchlistItems
                .Where(w => w.UserId == uid)
                .ToListAsync();

            var existingIds = existing.Select(w => w.MovieId).ToHashSet();
            var desiredSet = desired.ToHashSet();

            // Xoá những phim không còn trong danh sách mong muốn
            var toRemove = existing.Where(w => !desiredSet.Contains(w.MovieId)).ToList();
            if (toRemove.Count > 0)
                _context.WatchlistItems.RemoveRange(toRemove);

            // Thêm phim mới (giữ nguyên các bản ghi cũ để bảo toàn AddedAt)
            foreach (var id in desired.Where(id => !existingIds.Contains(id)))
            {
                _context.WatchlistItems.Add(new WatchlistItem
                {
                    UserId = uid,
                    MovieId = id,
                    AddedAt = System.DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
            return Ok(new { success = true, count = desired.Count });
        }

        private static List<int> ParseIds(string ids) =>
            ids.Split(',')
               .Select(x => int.TryParse(x.Trim(), out var n) ? n : 0)
               .Where(n => n > 0)
               .Distinct()
               .ToList();
    }
}

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using untitled1.Models.Entities;
using untitled1.Services;

namespace untitled1.Controllers
{
    public class ViewingHistoryController : Controller
    {
        private readonly IRecommendationService _recommendationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ViewingHistoryController(IRecommendationService recommendationService, UserManager<ApplicationUser> userManager)
        {
            _recommendationService = recommendationService;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> Log(int movieId, int watchTime)
        {
            if (movieId <= 0)
            {
                return Json(new { success = false, message = "Id phim không hợp lệ." });
            }

            if (User.Identity?.IsAuthenticated != true)
            {
                // We silently skip logging for anonymous users since UserId is required in the DB
                return Json(new { success = false, message = "Chưa đăng nhập." });
            }

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Không tìm thấy người dùng." });
            }

            await _recommendationService.LogWatchHistoryAsync(userId, movieId, watchTime);
            return Json(new { success = true });
        }
    }
}

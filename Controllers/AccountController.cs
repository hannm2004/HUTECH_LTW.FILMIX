using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using untitled1.Models.Entities;
using untitled1.Models.ViewModels;

namespace untitled1.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly untitled1.Services.ILogService _logService;

        public AccountController(
            SignInManager<ApplicationUser> signInManager, 
            UserManager<ApplicationUser> userManager,
            untitled1.Services.ILogService logService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logService = logService;
        }

        public IActionResult Auth()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([FromForm] LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Thông tin không hợp lệ" });

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    await _logService.LogAsync(user.Id, user.Email, "Login", "Đăng nhập thành công", HttpContext.Connection.RemoteIpAddress?.ToString());
                }
                return Json(new { success = true, redirectUrl = "/" });
            }

            await _logService.LogAsync(null, model.Email, "Login Failed", $"Đăng nhập thất bại với email {model.Email}", HttpContext.Connection.RemoteIpAddress?.ToString());
            return Json(new { success = false, message = "Email hoặc mật khẩu không đúng" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([FromForm] RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                return Json(new { success = false, message = string.Join("; ", errors) });
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _logService.LogAsync(user.Id, user.Email, "Register", "Đăng ký tài khoản mới thành công", HttpContext.Connection.RemoteIpAddress?.ToString());
                await _signInManager.SignInAsync(user, isPersistent: false);
                return Json(new { success = true, redirectUrl = "/" });
            }

            var identityErrors = string.Join("; ", result.Errors.Select(e => e.Description));
            await _logService.LogAsync(null, model.Email, "Register Failed", $"Đăng ký thất bại: {identityErrors}", HttpContext.Connection.RemoteIpAddress?.ToString());
            return Json(new { success = false, message = identityErrors });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                await _logService.LogAsync(user.Id, user.Email, "Logout", "Đăng xuất tài khoản", HttpContext.Connection.RemoteIpAddress?.ToString());
            }
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var db = HttpContext.RequestServices.GetRequiredService<untitled1.Data.ApplicationDbContext>();
            var activeSub = await db.UserSubscriptions
                .Include(s => s.Plan)
                .Where(s => s.UserId == user.Id && s.IsActive && s.EndDate > DateTime.Now)
                .OrderByDescending(s => s.EndDate)
                .FirstOrDefaultAsync();

            ViewBag.ActiveSubscription = activeSub;

            var orderRepo = HttpContext.RequestServices.GetRequiredService<untitled1.Repositories.IOrderRepository>();
            var orders = await orderRepo.GetByUserIdAsync(user.Id);
            ViewBag.Orders = orders;

            return View(user);
        }
    }
}
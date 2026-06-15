using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Security.Claims;
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
        private readonly untitled1.Services.ICartService _cartService;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            untitled1.Services.ILogService logService,
            untitled1.Services.ICartService cartService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logService = logService;
            _cartService = cartService;
        }

        public IActionResult Auth(string? returnUrl = null, string? error = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.AuthError = error switch
            {
                "external_unconfigured" => "Phương thức đăng nhập này chưa được cấu hình trên hệ thống. Vui lòng thử cách khác.",
                "external_noemail"      => "Nhà cung cấp không chia sẻ email nên không thể đăng nhập. Vui lòng thử cách khác.",
                "external_create"       => "Không thể tạo tài khoản từ đăng nhập mạng xã hội. Vui lòng thử lại.",
                "external"              => "Đăng nhập mạng xã hội thất bại. Vui lòng thử lại.",
                _                        => null
            };
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
                // M-04: Validate ReturnUrl to prevent Open Redirect attacks
                var redirectUrl = (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    ? model.ReturnUrl : "/";
                return Json(new { success = true, redirectUrl });
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
                // M-01: Assign default "User" role to all newly registered accounts
                await _userManager.AddToRoleAsync(user, "User");
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

            // Xoá giỏ hàng (cookie) để không lộ dữ liệu của tài khoản vừa đăng xuất sang trạng thái khách
            _cartService.ClearCart();

            return RedirectToAction("Index", "Home");
        }

        // ──────────────────────────────────────────────
        // SOCIAL LOGIN (Google / Facebook qua ASP.NET Identity)
        // ──────────────────────────────────────────────

        // GET /Account/ExternalLogin?provider=Google&returnUrl=/
        // Bắt đầu luồng OAuth: chuyển hướng người dùng tới nhà cung cấp.
        [HttpGet]
        public async Task<IActionResult> ExternalLogin(string provider, string? returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(provider))
                return RedirectToAction(nameof(Auth));

            // Nếu provider chưa được cấu hình credentials (scheme không tồn tại) → báo lỗi gọn thay vì 500
            var schemes = await _signInManager.GetExternalAuthenticationSchemesAsync();
            if (!schemes.Any(s => s.Name == provider))
                return RedirectToAction(nameof(Auth), new { error = "external_unconfigured" });

            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        // GET /Account/ExternalLoginCallback
        // Nhà cung cấp gọi lại sau khi xác thực. Đăng nhập nếu đã liên kết, ngược lại
        // tự tạo tài khoản (lần đầu) rồi liên kết external login và đăng nhập.
        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            if (remoteError != null)
                return RedirectToAction(nameof(Auth), new { error = "external" });

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
                return RedirectToAction(nameof(Auth), new { error = "external" });

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var fullName = info.Principal.FindFirstValue(ClaimTypes.Name)
                           ?? (string.IsNullOrEmpty(email) ? info.LoginProvider + " User" : email.Split('@')[0]);
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

            // 1) Đã từng liên kết external login này → đăng nhập trực tiếp
            var signInResult = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (signInResult.Succeeded)
            {
                var linked = email != null ? await _userManager.FindByEmailAsync(email) : null;
                await _logService.LogAsync(linked?.Id, email, "Login", $"Đăng nhập bằng {info.LoginProvider}", ip);
                return ExternalSuccess(fullName, email ?? string.Empty, returnUrl);
            }

            // Nhà cung cấp không trả về email → không thể tạo/đối chiếu tài khoản
            if (string.IsNullOrEmpty(email))
                return RedirectToAction(nameof(Auth), new { error = "external_noemail" });

            // 2) Có tài khoản cùng email → liên kết; 3) chưa có → tự tạo (auto-create lần đầu)
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = fullName,
                    EmailConfirmed = true // email đã được nhà cung cấp xác minh
                };
                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                    return RedirectToAction(nameof(Auth), new { error = "external_create" });

                await _userManager.AddToRoleAsync(user, "User");
                await _logService.LogAsync(user.Id, email, "Register", $"Tạo tài khoản qua {info.LoginProvider}", ip);
            }

            await _userManager.AddLoginAsync(user, info);
            await _signInManager.SignInAsync(user, isPersistent: false);
            await _logService.LogAsync(user.Id, email, "Login", $"Đăng nhập bằng {info.LoginProvider}", ip);

            return ExternalSuccess(fullName, email, returnUrl);
        }

        // Cầu nối: set localStorage 'filmix_user' (để navbar hiển thị đúng) rồi chuyển hướng.
        // Chống open-redirect bằng Url.IsLocalUrl.
        private IActionResult ExternalSuccess(string name, string email, string? returnUrl)
        {
            ViewBag.Name = name;
            ViewBag.Email = email;
            ViewBag.ReturnUrl = (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) ? returnUrl : "/";
            return View("ExternalLoginSuccess");
        }

        // GET /Account/AccessDenied
        // Called automatically by ASP.NET Identity when an authenticated user
        // tries to access a resource they are not authorized for.
        public IActionResult AccessDenied()
        {
            return View();
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
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using untitled1.Models.DTOs;
using untitled1.Models.Entities;
using untitled1.Models.Settings;
using untitled1.Services;

namespace untitled1.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthApiController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtService _jwtService;
        private readonly ILogService _logService;
        private readonly JwtSettings _jwtSettings;

        public AuthApiController(
            UserManager<ApplicationUser> userManager,
            IJwtService jwtService,
            ILogService logService,
            IOptions<JwtSettings> jwtSettings)
        {
            _userManager = userManager;
            _jwtService = jwtService;
            _logService = logService;
            _jwtSettings = jwtSettings.Value;
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (request == null || !ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Thông tin đăng nhập không hợp lệ.", ModelState));
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                // Ghi Audit Log "API Login Failed"
                await _logService.LogAsync(
                    userId: null,
                    userName: request?.Email ?? "Anonymous",
                    action: "API Login Failed",
                    description: $"Đăng nhập REST API thất bại: Sai email hoặc mật khẩu cho tài khoản {request?.Email}",
                    ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString()
                );

                return Unauthorized(ApiResponse<object>.ErrorResponse("Email hoặc mật khẩu không chính xác."));
            }

            var token = await _jwtService.GenerateAccessToken(user);
            var roles = await _userManager.GetRolesAsync(user);
            var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes);

            // Ghi Audit Log "API Login"
            await _logService.LogAsync(
                userId: user.Id,
                userName: user.UserName,
                action: "API Login",
                description: $"Đăng nhập REST API thành công bằng tài khoản: {request.Email}",
                ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString()
            );

            var response = new LoginResponseDto
            {
                Token = token,
                ExpiresAt = expiresAt,
                UserName = user.UserName ?? string.Empty,
                Roles = roles
            };

            return Ok(response);
        }

        // GET: api/auth/profile
        [HttpGet("profile")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse("Token không chứa thông tin người dùng hợp lệ."));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("Không tìm thấy người dùng."));
            }

            var roles = await _userManager.GetRolesAsync(user);
            bool isPremium = user.PremiumEndDate.HasValue && user.PremiumEndDate.Value > DateTime.Now;

            var profileDto = new UserProfileDto
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                Roles = roles,
                IsPremium = isPremium
            };

            return Ok(profileDto);
        }
    }
}

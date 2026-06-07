using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using untitled1.Models.DTOs;
using untitled1.Models.Entities;
using untitled1.Repositories;
using untitled1.Services;

namespace untitled1.Controllers
{
    [ApiController]
    [Route("api/cart")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CartApiController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly ISubscriptionPlanRepository _planRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartApiController(
            ICartService cartService,
            ISubscriptionPlanRepository planRepository,
            UserManager<ApplicationUser> userManager)
        {
            _cartService = cartService;
            _planRepository = planRepository;
            _userManager = userManager;
        }

        private async Task<CartDto> GetCartDtoAsync()
        {
            var cartItems = _cartService.GetCart();
            var itemsDto = cartItems.Select(item => new CartItemDto
            {
                PlanId = item.PlanId,
                PlanName = item.PlanName,
                Price = item.Price,
                Quantity = item.Quantity,
                AccentColor = item.AccentColor,
                Resolution = item.Resolution,
                TotalPrice = item.TotalPrice
            }).ToList();

            var totalAmount = _cartService.GetTotalAmount();
            var totalQuantity = itemsDto.Sum(item => item.Quantity);

            UserInfoDto? userDto = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    bool isPremium = user.PremiumEndDate.HasValue && user.PremiumEndDate.Value > DateTime.Now;
                    userDto = new UserInfoDto
                    {
                        Id = user.Id,
                        Username = user.UserName ?? string.Empty,
                        Email = user.Email ?? string.Empty,
                        FullName = user.FullName,
                        IsPremium = isPremium
                    };
                }
            }

            return new CartDto
            {
                Items = itemsDto,
                TotalAmount = totalAmount,
                TotalQuantity = totalQuantity,
                User = userDto
            };
        }

        // GET: api/cart
        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var cartDto = await GetCartDtoAsync();
            return Ok(ApiResponse<CartDto>.SuccessResponse(cartDto, "Lấy thông tin giỏ hàng thành công."));
        }

        // POST: api/cart/items
        [HttpPost("items")]
        public async Task<IActionResult> AddItem([FromBody] AddToCartDto dto)
        {
            var plan = await _planRepository.GetByIdAsync(dto.PlanId);
            if (plan == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse($"Gói dịch vụ có mã {dto.PlanId} không tồn tại."));
            }

            _cartService.AddToCart(plan.Id, plan.Name, plan.Price, plan.AccentColor, plan.Resolution);
            var updatedCart = await GetCartDtoAsync();

            return Ok(ApiResponse<CartDto>.SuccessResponse(updatedCart, "Thêm sản phẩm vào giỏ hàng thành công."));
        }

        // PUT: api/cart/items/{planId}
        [HttpPut("items/{planId}")]
        public async Task<IActionResult> UpdateQuantity([FromRoute] int planId, [FromBody] UpdateQuantityDto dto)
        {
            var cart = _cartService.GetCart();
            var item = cart.FirstOrDefault(i => i.PlanId == planId);
            if (item == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse($"Sản phẩm có mã {planId} không tồn tại trong giỏ hàng."));
            }

            _cartService.UpdateQuantity(planId, dto.Quantity);
            var updatedCart = await GetCartDtoAsync();

            return Ok(ApiResponse<CartDto>.SuccessResponse(updatedCart, "Cập nhật số lượng sản phẩm thành công."));
        }

        // DELETE: api/cart/items/{planId}
        [HttpDelete("items/{planId}")]
        public async Task<IActionResult> RemoveItem([FromRoute] int planId)
        {
            var cart = _cartService.GetCart();
            var item = cart.FirstOrDefault(i => i.PlanId == planId);
            if (item == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse($"Sản phẩm có mã {planId} không tồn tại trong giỏ hàng."));
            }

            _cartService.RemoveFromCart(planId);
            var updatedCart = await GetCartDtoAsync();

            return Ok(ApiResponse<CartDto>.SuccessResponse(updatedCart, "Xóa sản phẩm khỏi giỏ hàng thành công."));
        }

        // DELETE: api/cart
        [HttpDelete]
        public async Task<IActionResult> ClearCart()
        {
            _cartService.ClearCart();
            var updatedCart = await GetCartDtoAsync();

            return Ok(ApiResponse<CartDto>.SuccessResponse(updatedCart, "Xóa toàn bộ giỏ hàng thành công."));
        }
    }
}

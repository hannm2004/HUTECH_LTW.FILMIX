using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using untitled1.Models.ViewModels;

namespace untitled1.Services
{
    public class CartService : ICartService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string GuestCartCookieKey = "FilmixCart_guest";

        public CartService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private HttpContext? Context => _httpContextAccessor.HttpContext;

        // Mỗi tài khoản có giỏ hàng riêng: cookie được đặt tên theo UserId khi đã đăng nhập,
        // khách (chưa đăng nhập) dùng cookie "guest" tách biệt. Nhờ vậy giỏ của khách KHÔNG
        // lẫn vào giỏ của tài khoản sau khi đăng nhập, và mỗi tài khoản chỉ thấy giỏ của mình.
        private string CartCookieKey
        {
            get
            {
                var user = Context?.User;
                if (user?.Identity?.IsAuthenticated == true)
                {
                    var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (!string.IsNullOrEmpty(userId))
                        return $"FilmixCart_{userId}";
                }
                return GuestCartCookieKey;
            }
        }

        public List<CartItemViewModel> GetCart()
        {
            var raw = Context?.Request.Cookies[CartCookieKey];
            if (string.IsNullOrEmpty(raw))
            {
                return new List<CartItemViewModel>();
            }

            try
            {
                // Cookie chứa JSON đã được Base64 hoá để an toàn ký tự
                var json = Encoding.UTF8.GetString(Convert.FromBase64String(raw));
                return JsonSerializer.Deserialize<List<CartItemViewModel>>(json) ?? new List<CartItemViewModel>();
            }
            catch
            {
                return new List<CartItemViewModel>();
            }
        }

        public void AddToCart(int planId, string name, decimal price, string accentColor, string resolution)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(i => i.PlanId == planId);

            if (item != null)
            {
                item.Quantity++;
            }
            else
            {
                cart.Add(new CartItemViewModel
                {
                    PlanId = planId,
                    PlanName = name,
                    Price = price,
                    Quantity = 1,
                    AccentColor = accentColor,
                    Resolution = resolution
                });
            }

            SaveCart(cart);
        }

        public void UpdateQuantity(int planId, int quantity)
        {
            if (quantity <= 0)
            {
                RemoveFromCart(planId);
                return;
            }

            var cart = GetCart();
            var item = cart.FirstOrDefault(i => i.PlanId == planId);

            if (item != null)
            {
                item.Quantity = quantity;
                SaveCart(cart);
            }
        }

        public void RemoveFromCart(int planId)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(i => i.PlanId == planId);

            if (item != null)
            {
                cart.Remove(item);
                SaveCart(cart);
            }
        }

        public void ClearCart()
        {
            Context?.Response.Cookies.Delete(CartCookieKey);
        }

        public decimal GetTotalAmount()
        {
            return GetCart().Sum(i => i.TotalPrice);
        }

        private void SaveCart(List<CartItemViewModel> cart)
        {
            if (Context == null) return;

            if (cart.Count == 0)
            {
                Context.Response.Cookies.Delete(CartCookieKey);
                return;
            }

            var json = JsonSerializer.Serialize(cart);
            var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

            // Cookie bền: sống sót qua đăng nhập/đăng xuất và khởi động lại app
            Context.Response.Cookies.Append(CartCookieKey, encoded, new CookieOptions
            {
                HttpOnly = true,
                IsEssential = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(30)
            });
        }
    }
}

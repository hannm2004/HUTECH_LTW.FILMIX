using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using untitled1.Models.ViewModels;

namespace untitled1.Services
{
    public class CartService : ICartService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string CartSessionKey = "FilmixCart";

        public CartService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ISession? Session => _httpContextAccessor.HttpContext?.Session;

        public List<CartItemViewModel> GetCart()
        {
            if (Session == null) return new List<CartItemViewModel>();

            var json = Session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(json))
            {
                return new List<CartItemViewModel>();
            }

            try
            {
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
            if (Session != null)
            {
                Session.Remove(CartSessionKey);
            }
        }

        public decimal GetTotalAmount()
        {
            return GetCart().Sum(i => i.TotalPrice);
        }

        private void SaveCart(List<CartItemViewModel> cart)
        {
            if (Session != null)
            {
                var json = JsonSerializer.Serialize(cart);
                Session.SetString(CartSessionKey, json);
            }
        }
    }
}

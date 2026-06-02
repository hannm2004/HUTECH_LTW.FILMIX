using System.Collections.Generic;
using untitled1.Models.ViewModels;

namespace untitled1.Services
{
    public interface ICartService
    {
        List<CartItemViewModel> GetCart();
        void AddToCart(int planId, string name, decimal price, string accentColor, string resolution);
        void UpdateQuantity(int planId, int quantity);
        void RemoveFromCart(int planId);
        void ClearCart();
        decimal GetTotalAmount();
    }
}

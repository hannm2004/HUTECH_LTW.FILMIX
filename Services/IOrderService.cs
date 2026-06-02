using System.Threading.Tasks;
using untitled1.Models.Entities;
using untitled1.Models.ViewModels;

namespace untitled1.Services
{
    public interface IOrderService
    {
        Task<Order> CreateOrderAsync(CheckoutViewModel model, string userId);
        Task<bool> ProcessPaymentAsync(int orderId, string paymentMethod);
        Task<bool> ApprovePaymentAsync(int orderId);
        Task<bool> CancelOrderAsync(int orderId);
        Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status);
        Task<int> SyncSubscriptionLifecyclesAsync();
    }
}

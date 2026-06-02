using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using untitled1.Data;
using untitled1.Models.Entities;
using untitled1.Models.ViewModels;
using untitled1.Repositories;

namespace untitled1.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ISubscriptionPlanRepository _planRepository;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderService(
            IOrderRepository orderRepository,
            ISubscriptionPlanRepository planRepository,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _orderRepository = orderRepository;
            _planRepository = planRepository;
            _context = context;
            _userManager = userManager;
        }

        public async Task<Order> CreateOrderAsync(CheckoutViewModel model, string userId)
        {
            var order = new Order
            {
                UserId = userId,
                FullName = model.FullName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Address = model.Address,
                Notes = model.Notes,
                PaymentMethod = model.PaymentMethod,
                TotalAmount = model.TotalAmount,
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.Now
            };

            foreach (var item in model.CartItems)
            {
                order.OrderItems.Add(new OrderItem
                {
                    PlanId = item.PlanId,
                    Quantity = item.Quantity,
                    Price = item.Price
                });
            }

            await _orderRepository.AddAsync(order);
            await _orderRepository.SaveAsync();

            return order;
        }

        public async Task<bool> ProcessPaymentAsync(int orderId, string paymentMethod)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null) return false;

            order.PaymentMethod = paymentMethod;

            // VNPay or PayOS mock payments: automatic transition to Paid
            if (paymentMethod == "VNPay" || paymentMethod == "PayOS")
            {
                order.Status = OrderStatus.Paid;
                await _orderRepository.UpdateAsync(order);
                await _orderRepository.SaveAsync();

                // Activate Premium
                await ActivatePremiumSubscriptionAsync(order);
            }
            else
            {
                // COD or Bank Transfer - remains Pending until Admin approves
                order.Status = OrderStatus.Pending;
                await _orderRepository.UpdateAsync(order);
                await _orderRepository.SaveAsync();
            }

            return true;
        }

        public async Task<bool> ApprovePaymentAsync(int orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null || order.Status == OrderStatus.Completed || order.Status == OrderStatus.Paid) 
                return false;

            order.Status = OrderStatus.Paid;
            await _orderRepository.UpdateAsync(order);
            await _orderRepository.SaveAsync();

            // Activate Premium
            await ActivatePremiumSubscriptionAsync(order);

            return true;
        }

        public async Task<bool> CancelOrderAsync(int orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null || order.Status == OrderStatus.Completed || order.Status == OrderStatus.Cancelled) 
                return false;

            order.Status = OrderStatus.Cancelled;
            await _orderRepository.UpdateAsync(order);
            await _orderRepository.SaveAsync();

            return true;
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null) return false;

            order.Status = status;
            await _orderRepository.UpdateAsync(order);
            await _orderRepository.SaveAsync();

            // If transition to Paid or Completed, make sure subscription is activated
            if (status == OrderStatus.Paid || status == OrderStatus.Completed)
            {
                await ActivatePremiumSubscriptionAsync(order);
            }

            return true;
        }

        public async Task<int> SyncSubscriptionLifecyclesAsync()
        {
            var now = DateTime.Now;
            var expiredSubs = await _context.UserSubscriptions
                .Where(s => s.IsActive && s.EndDate < now)
                .ToListAsync();

            int updatedCount = 0;
            foreach (var sub in expiredSubs)
            {
                sub.IsActive = false;
                _context.UserSubscriptions.Update(sub);
                updatedCount++;

                var user = await _userManager.FindByIdAsync(sub.UserId);
                if (user != null && user.PremiumEndDate <= now)
                {
                    var hasNewer = await _context.UserSubscriptions
                        .AnyAsync(s => s.UserId == user.Id && s.IsActive && s.EndDate > now);
                    if (!hasNewer)
                    {
                        user.PremiumStartDate = null;
                        user.PremiumEndDate = null;
                        await _userManager.UpdateAsync(user);
                    }
                }
            }

            if (updatedCount > 0)
            {
                await _context.SaveChangesAsync();
            }

            return updatedCount;
        }

        private async Task ActivatePremiumSubscriptionAsync(Order order)
        {
            var user = await _userManager.FindByIdAsync(order.UserId);
            if (user == null) return;

            // Find all item plans in this order to activate
            foreach (var item in order.OrderItems)
            {
                var plan = await _planRepository.GetByIdAsync(item.PlanId);
                if (plan == null) continue;

                // Deactivate any existing active subscriptions first, or extend if it's same plan
                var activeSub = await _context.UserSubscriptions
                    .FirstOrDefaultAsync(s => s.UserId == user.Id && s.IsActive && s.EndDate > DateTime.Now);

                DateTime startDate = DateTime.Now;
                DateTime endDate = DateTime.Now.AddMonths(item.Quantity);

                if (activeSub != null)
                {
                    if (activeSub.PlanId == item.PlanId)
                    {
                        // Same plan: Extend
                        activeSub.EndDate = activeSub.EndDate.AddMonths(item.Quantity);
                        endDate = activeSub.EndDate;
                        startDate = activeSub.StartDate;
                        _context.UserSubscriptions.Update(activeSub);
                    }
                    else
                    {
                        // Different plan: Deactivate old and create new
                        activeSub.IsActive = false;
                        _context.UserSubscriptions.Update(activeSub);

                        var newSub = new UserSubscription
                        {
                            UserId = user.Id,
                            PlanId = item.PlanId,
                            StartDate = startDate,
                            EndDate = endDate,
                            IsActive = true,
                            PaymentMethod = order.PaymentMethod,
                            TransactionId = "ORD-" + order.Id + "-" + DateTime.Now.Ticks.ToString()[^6..],
                            CreatedAt = DateTime.Now
                        };
                        await _context.UserSubscriptions.AddAsync(newSub);
                    }
                }
                else
                {
                    // No active sub: Create new
                    var newSub = new UserSubscription
                    {
                        UserId = user.Id,
                        PlanId = item.PlanId,
                        StartDate = startDate,
                        EndDate = endDate,
                        IsActive = true,
                        PaymentMethod = order.PaymentMethod,
                        TransactionId = "ORD-" + order.Id + "-" + DateTime.Now.Ticks.ToString()[^6..],
                        CreatedAt = DateTime.Now
                    };
                    await _context.UserSubscriptions.AddAsync(newSub);
                }

                // Update ApplicationUser fields
                user.PremiumStartDate = startDate;
                user.PremiumEndDate = endDate;
                await _userManager.UpdateAsync(user);
            }

            await _context.SaveChangesAsync();
        }
    }
}

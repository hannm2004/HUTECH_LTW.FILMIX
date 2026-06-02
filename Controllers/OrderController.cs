using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using untitled1.Models.Entities;
using untitled1.Models.ViewModels;
using untitled1.Services;
using untitled1.Repositories;

namespace untitled1.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        private readonly IOrderRepository _orderRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(
            ICartService cartService,
            IOrderService orderService,
            IOrderRepository orderRepository,
            UserManager<ApplicationUser> userManager)
        {
            _cartService = cartService;
            _orderService = orderService;
            _orderRepository = orderRepository;
            _userManager = userManager;
        }

        // GET /Order/Checkout
        public async Task<IActionResult> Checkout()
        {
            var cart = _cartService.GetCart();
            if (cart.Count == 0)
            {
                return RedirectToAction("Index", "Cart");
            }

            var user = await _userManager.GetUserAsync(User);
            var model = new CheckoutViewModel
            {
                CartItems = cart,
                TotalAmount = _cartService.GetTotalAmount(),
                Email = user?.Email ?? string.Empty,
                FullName = user?.FullName ?? string.Empty,
                PhoneNumber = user?.PhoneNumber ?? string.Empty
            };

            return View(model);
        }

        // POST /Order/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            var cart = _cartService.GetCart();
            if (cart.Count == 0)
            {
                return RedirectToAction("Index", "Cart");
            }

            // Populate the cart items and total back to model in case validation fails and we need to redisplay
            model.CartItems = cart;
            model.TotalAmount = _cartService.GetTotalAmount();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = _userManager.GetUserId(User)!;
            var order = await _orderService.CreateOrderAsync(model, userId);

            // Clear the cart
            _cartService.ClearCart();

            // Process payment method
            await _orderService.ProcessPaymentAsync(order.Id, model.PaymentMethod);

            return RedirectToAction("Payment", new { orderId = order.Id });
        }

        // GET /Order/Payment
        public async Task<IActionResult> Payment(int orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (order.UserId != userId) return Forbid();

            return View(order);
        }

        // POST /Order/ProcessMockPayment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessMockPayment(int orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (order.UserId != userId) return Forbid();

            // Set order as Paid (since mock VNPay, mock PayOS, or mock bank transfer is completed by user clicking "Confirm")
            await _orderService.UpdateOrderStatusAsync(orderId, OrderStatus.Paid);

            return RedirectToAction("Success", new { orderId = order.Id });
        }

        // GET /Order/Success
        public async Task<IActionResult> Success(int orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (order.UserId != userId) return Forbid();

            return View(order);
        }

        // GET /Order/History
        public async Task<IActionResult> History()
        {
            var userId = _userManager.GetUserId(User)!;
            var orders = await _orderRepository.GetByUserIdAsync(userId);
            return View(orders);
        }
    }
}

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
        private readonly IEmailService _emailService;

        public OrderController(
            ICartService cartService,
            IOrderService orderService,
            IOrderRepository orderRepository,
            UserManager<ApplicationUser> userManager,
            IEmailService emailService)
        {
            _cartService = cartService;
            _orderService = orderService;
            _orderRepository = orderRepository;
            _userManager = userManager;
            _emailService = emailService;
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

            // Đơn được tạo ở trạng thái Pending với đúng phương thức đã chọn.
            // KHÔNG xử lý thanh toán tại đây — thanh toán & kích hoạt Premium diễn ra ở
            // Payment/ProcessMockPayment (idempotent) để tránh xử lý sớm và trùng lặp.
            //
            // Gửi email xác nhận ĐƠN HÀNG đúng 1 lần ngay khi đặt hàng, áp dụng cho MỌI
            // phương thức (kể cả COD vốn không đi qua ProcessMockPayment). Tải lại đơn kèm
            // OrderItems/Plan để email có đủ dữ liệu.
            var fullOrder = await _orderRepository.GetByIdAsync(order.Id);
            if (fullOrder != null)
                _ = _emailService.SendOrderConfirmationAsync(fullOrder);

            return RedirectToAction("Payment", new { orderId = order.Id });
        }

        // GET /Order/Payment
        public async Task<IActionResult> Payment(int orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (order.UserId != userId) return Forbid();

            // Đơn đã thanh toán → về thẳng trang Success (tránh lỗi khi bấm Back vào lại trang thanh toán)
            if (order.Status == OrderStatus.Paid || order.Status == OrderStatus.Completed)
            {
                return RedirectToAction("Success", new { orderId = order.Id });
            }

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

            // Idempotent: chỉ chuyển sang Paid & kích hoạt Premium nếu đơn chưa được thanh toán.
            // Thanh toán xử lý đúng 1 lần; kích hoạt Premium (trong UpdateOrderStatusAsync) cũng 1 lần.
            // Email xác nhận đã được gửi 1 lần ở POST Checkout nên KHÔNG gửi lại ở đây.
            if (order.Status != OrderStatus.Paid && order.Status != OrderStatus.Completed)
            {
                await _orderService.UpdateOrderStatusAsync(orderId, OrderStatus.Paid);
            }

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

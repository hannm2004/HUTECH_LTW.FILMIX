using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<OrderController> _logger;

        public OrderController(
            ICartService cartService,
            IOrderService orderService,
            IOrderRepository orderRepository,
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            ILogger<OrderController> logger)
        {
            _cartService = cartService;
            _orderService = orderService;
            _orderRepository = orderRepository;
            _userManager = userManager;
            _emailService = emailService;
            _logger = logger;
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

            // COD không đi qua bước ProcessMockPayment (trang Payment chỉ hiển thị xác nhận),
            // nên gửi email xác nhận đơn hàng ngay tại đây để mọi phương thức đều gửi mail
            // về địa chỉ người dùng đã điền. Các phương thức online gửi mail ở ProcessMockPayment.
            if (string.Equals(order.PaymentMethod, "COD", StringComparison.OrdinalIgnoreCase))
            {
                var codOrder = await _orderRepository.GetByIdAsync(order.Id);
                if (codOrder != null)
                {
                    _logger.LogInformation(
                        "[OrderController] Đơn COD #{OrderId} đã tạo. Đang gửi email xác nhận tới {Email}...",
                        codOrder.Id, codOrder.Email);
                    SendOrderConfirmationInBackground(codOrder);
                }
            }

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

            // Idempotent: chỉ chuyển sang Paid, kích hoạt Premium VÀ gửi email khi đơn CHƯA thanh toán.
            // → mỗi thứ (thanh toán / kích hoạt Premium / email) diễn ra đúng 1 lần; các lần gọi lại
            //   (bấm Back, refresh, double-submit) đều bị bỏ qua, và không bao giờ gửi khi đơn còn Pending.
            if (order.Status != OrderStatus.Paid && order.Status != OrderStatus.Completed)
            {
                await _orderService.UpdateOrderStatusAsync(orderId, OrderStatus.Paid);

                // Tải lại đơn (đã Paid, kèm OrderItems/Plan) để đảm bảo navigation properties được load đầy đủ
                var paidOrder = await _orderRepository.GetByIdAsync(orderId);
                if (paidOrder != null)
                {
                    _logger.LogInformation(
                        "[OrderController] Đơn hàng #{OrderId} đã thanh toán thành công. Đang gửi email xác nhận tới {Email}...",
                        paidOrder.Id, paidOrder.Email);

                    SendOrderConfirmationInBackground(paidOrder);
                }
            }

            return RedirectToAction("Success", new { orderId = order.Id });
        }

        // Gửi email xác nhận đơn hàng ở background task — không làm chậm redirect của người dùng,
        // vẫn log đầy đủ nếu SMTP lỗi. Đơn (kèm OrderItems/Plan) đã được nạp sẵn trên luồng request
        // trước khi truyền vào đây để tránh dùng DbContext (scoped) sau khi request kết thúc.
        private void SendOrderConfirmationInBackground(Order order)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await _emailService.SendOrderConfirmationAsync(order);
                    _logger.LogInformation(
                        "[OrderController] Email xác nhận đơn hàng #{OrderId} đã được gửi tới {Email}.",
                        order.Id, order.Email);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "[OrderController] Gửi email xác nhận thất bại cho đơn hàng #{OrderId} - Email: {Email}",
                        order.Id, order.Email);
                }
            });
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

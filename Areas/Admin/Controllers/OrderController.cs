using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using untitled1.Models.Entities;
using untitled1.Repositories;
using untitled1.Services;

namespace untitled1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrderController : Controller
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderService _orderService;

        public OrderController(IOrderRepository orderRepository, IOrderService orderService)
        {
            _orderRepository = orderRepository;
            _orderService = orderService;
        }

        // GET /Admin/Order
        public async Task<IActionResult> Index()
        {
            var orders = await _orderRepository.GetAllAsync();
            return View(orders);
        }

        // POST /Admin/Order/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
        {
            var success = await _orderService.UpdateOrderStatusAsync(id, status);
            if (!success)
            {
                TempData["ErrorMessage"] = "Cập nhật trạng thái thất bại.";
            }
            else
            {
                TempData["SuccessMessage"] = $"Đã cập nhật trạng thái đơn hàng #{id} thành công.";
            }
            return RedirectToAction("Index");
        }

        // POST /Admin/Order/SyncLifecycles
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SyncLifecycles()
        {
            var count = await _orderService.SyncSubscriptionLifecyclesAsync();
            TempData["SuccessMessage"] = $"Đồng bộ vòng đời thành công. Đã quét và cập nhật {count} gói hết hạn.";
            return RedirectToAction("Index");
        }
    }
}

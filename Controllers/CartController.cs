using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using untitled1.Repositories;
using untitled1.Services;

namespace untitled1.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly ISubscriptionPlanRepository _planRepository;

        public CartController(ICartService cartService, ISubscriptionPlanRepository planRepository)
        {
            _cartService = cartService;
            _planRepository = planRepository;
        }

        // GET /Cart
        public IActionResult Index()
        {
            var cart = _cartService.GetCart();
            ViewBag.TotalAmount = _cartService.GetTotalAmount();
            return View(cart);
        }

        // POST /Cart/Add
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Add(int planId)
        {
            var plan = await _planRepository.GetByIdAsync(planId);
            if (plan == null) return NotFound();

            _cartService.AddToCart(plan.Id, plan.Name, plan.Price, plan.AccentColor, plan.Resolution);
            return RedirectToAction("Index");
        }

        // POST /Cart/UpdateQuantity
        [Authorize]
        [HttpPost]
        public IActionResult UpdateQuantity(int planId, int quantity)
        {
            _cartService.UpdateQuantity(planId, quantity);
            return RedirectToAction("Index");
        }

        // POST /Cart/Remove
        [Authorize]
        [HttpPost]
        public IActionResult Remove(int planId)
        {
            _cartService.RemoveFromCart(planId);
            return RedirectToAction("Index");
        }

        // POST /Cart/Clear
        [Authorize]
        [HttpPost]
        public IActionResult Clear()
        {
            _cartService.ClearCart();
            return RedirectToAction("Index");
        }
    }
}

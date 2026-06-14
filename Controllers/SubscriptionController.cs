using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using untitled1.Data;
using untitled1.Models.Entities;
using untitled1.Services;

namespace untitled1.Controllers
{
    public class SubscriptionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogService _logService;
        private readonly ICartService _cartService;

        public SubscriptionController(
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager,
            ILogService logService,
            ICartService cartService)
        {
            _context = context;
            _userManager = userManager;
            _logService = logService;
            _cartService = cartService;
        }

        // GET /Subscription/Plans
        public async Task<IActionResult> Plans()
        {
            var plans = await _context.SubscriptionPlans.OrderBy(p => p.Price).ToListAsync();
            if (User.Identity?.IsAuthenticated == true)
            {
                var uid = _userManager.GetUserId(User);
                ViewBag.ActiveSubscription = await _context.UserSubscriptions
                    .Include(s => s.Plan)
                    .FirstOrDefaultAsync(s => s.UserId == uid && s.IsActive && s.EndDate > DateTime.Now);
            }
            return View(plans);
        }

        // GET /Subscription/Checkout/2 — M-05: Route through standard Cart → Order flow
        [Authorize]
        public async Task<IActionResult> Checkout(int planId)
        {
            var plan = await _context.SubscriptionPlans.FindAsync(planId);
            if (plan == null) return NotFound();

            // Add the chosen plan to the cart and redirect to the unified checkout page
            _cartService.ClearCart(); // Start with clean cart for subscription
            _cartService.AddToCart(plan.Id, plan.Name, plan.Price, plan.AccentColor, plan.Resolution);

            return RedirectToAction("Checkout", "Order");
        }



        // GET /Subscription/MySubscription
        [Authorize]
        public async Task<IActionResult> MySubscription()
        {
            var uid = _userManager.GetUserId(User);
            var sub = await _context.UserSubscriptions.Include(s => s.Plan)
                .Where(s => s.UserId == uid).OrderByDescending(s => s.CreatedAt).FirstOrDefaultAsync();
            return View(sub);
        }

        // GET /api/subscription/status
        [HttpGet("/api/subscription/status")]
        [Authorize]
        public async Task<IActionResult> ApiStatus()
        {
            var uid = _userManager.GetUserId(User);
            var sub = await _context.UserSubscriptions.Include(s => s.Plan)
                .FirstOrDefaultAsync(s => s.UserId == uid && s.IsActive && s.EndDate > DateTime.Now);
            if (sub == null) return Json(new { active = false });
            return Json(new { active = true, planName = sub.Plan.Name, planColor = sub.Plan.AccentColor, endDate = sub.EndDate.ToString("dd/MM/yyyy") });
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using untitled1.Data;
using untitled1.Models.Entities;

namespace untitled1.Controllers
{
    public class SubscriptionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SubscriptionController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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

        // GET /Subscription/Checkout/2
        [Authorize]
        public async Task<IActionResult> Checkout(int planId)
        {
            var plan = await _context.SubscriptionPlans.FindAsync(planId);
            if (plan == null) return NotFound();
            var uid = _userManager.GetUserId(User);
            ViewBag.ActiveSubscription = await _context.UserSubscriptions
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s => s.UserId == uid && s.IsActive && s.EndDate > DateTime.Now);
            return View(plan);
        }

        // POST /Subscription/ProcessPayment
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(int planId, string paymentMethod)
        {
            var plan = await _context.SubscriptionPlans.FindAsync(planId);
            if (plan == null) return NotFound();
            var uid = _userManager.GetUserId(User)!;
            var existing = await _context.UserSubscriptions.Where(s => s.UserId == uid && s.IsActive).ToListAsync();
            foreach (var s in existing) s.IsActive = false;
            var sub = new UserSubscription
            {
                UserId = uid, PlanId = planId,
                StartDate = DateTime.Now, EndDate = DateTime.Now.AddMonths(1),
                IsActive = true, PaymentMethod = paymentMethod ?? "credit_card",
                TransactionId = "FX" + DateTime.Now.Ticks.ToString()[^8..],
                CreatedAt = DateTime.Now
            };
            _context.UserSubscriptions.Add(sub);
            await _context.SaveChangesAsync();
            return RedirectToAction("Success", new { id = sub.Id });
        }

        // GET /Subscription/Success/5
        [Authorize]
        public async Task<IActionResult> Success(int id)
        {
            var sub = await _context.UserSubscriptions.Include(s => s.Plan).FirstOrDefaultAsync(s => s.Id == id);
            if (sub == null) return NotFound();
            return View(sub);
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

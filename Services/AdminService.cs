using System;
using System.Collections.Generic;
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
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext  _context;
        private readonly UserManager<ApplicationUser>  _userManager;
        private readonly RoleManager<IdentityRole>     _roleManager;
        private readonly ISubscriptionRepository       _subscriptionRepo;
        private readonly IOrderRepository              _orderRepo;

        public AdminService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ISubscriptionRepository subscriptionRepo,
            IOrderRepository orderRepo)
        {
            _context          = context;
            _userManager      = userManager;
            _roleManager      = roleManager;
            _subscriptionRepo = subscriptionRepo;
            _orderRepo        = orderRepo;
        }

        // ──────────────────────────────────────────
        // DASHBOARD
        // ──────────────────────────────────────────

        public async Task<DashboardViewModel> GetDashboardDataAsync()
        {
            var now      = DateTime.UtcNow;
            var thisMonth = new DateTime(now.Year, now.Month, 1);
            var lastMonth = thisMonth.AddMonths(-1);

            var allOrders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Plan)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var allSubs = await _context.UserSubscriptions
                .Include(s => s.User)
                .Include(s => s.Plan)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            // KPIs
            var totalRevenue  = allOrders.Where(o => o.Status == OrderStatus.Completed || o.Status == OrderStatus.Paid)
                                         .Sum(o => o.TotalAmount);
            var monthRevenue  = allOrders.Where(o => o.CreatedAt >= thisMonth &&
                                                      (o.Status == OrderStatus.Completed || o.Status == OrderStatus.Paid))
                                         .Sum(o => o.TotalAmount);
            var lastMonthRev  = allOrders.Where(o => o.CreatedAt >= lastMonth && o.CreatedAt < thisMonth &&
                                                      (o.Status == OrderStatus.Completed || o.Status == OrderStatus.Paid))
                                         .Sum(o => o.TotalAmount);

            var totalUsers    = await _context.Users.CountAsync();
            var lastMonthUsers = await _context.Users.CountAsync(); // simplified — no CreatedAt on IdentityUser
            var activeSubs    = allSubs.Count(s => s.IsActive && s.EndDate >= now);

            // Monthly chart (last 6 months)
            var revenueLabels = new List<string>();
            var revenueData   = new List<decimal>();
            var userLabels    = new List<string>();
            var userData      = new List<int>();

            for (int i = 5; i >= 0; i--)
            {
                var month = thisMonth.AddMonths(-i);
                var next  = month.AddMonths(1);
                revenueLabels.Add(month.ToString("MM/yyyy"));
                revenueData.Add(allOrders
                    .Where(o => o.CreatedAt >= month && o.CreatedAt < next &&
                                (o.Status == OrderStatus.Completed || o.Status == OrderStatus.Paid))
                    .Sum(o => o.TotalAmount));

                userLabels.Add(month.ToString("MM/yyyy"));
                // new subscriptions per month as proxy for user growth
                userData.Add(allSubs.Count(s => s.CreatedAt >= month && s.CreatedAt < next));
            }

            return new DashboardViewModel
            {
                TotalMovies        = await _context.Movies.CountAsync(),
                TotalTVSeries      = await _context.Movies.CountAsync(m => m.IsTVSeries),
                TotalFilms         = await _context.Movies.CountAsync(m => !m.IsTVSeries),
                TotalUsers         = totalUsers,
                TotalOrders        = allOrders.Count,
                PendingOrders      = allOrders.Count(o => o.Status == OrderStatus.Pending),
                ActiveSubscriptions = activeSubs,
                TotalRevenue       = totalRevenue,
                MonthRevenue       = monthRevenue,
                RevenueGrowthPct   = lastMonthRev == 0 ? 100 : Math.Round((double)((monthRevenue - lastMonthRev) / lastMonthRev * 100), 1),
                UserGrowthPct      = 0, // placeholder
                RecentOrders        = allOrders.Take(8),
                RecentSubscriptions = allSubs.Take(6),
                RecentMovies        = await _context.Movies.OrderByDescending(m => m.Id).Take(5).ToListAsync(),
                RevenueChartLabels  = revenueLabels,
                RevenueChartData    = revenueData,
                UserChartLabels     = userLabels,
                UserChartData       = userData,
            };
        }

        // ──────────────────────────────────────────
        // USERS
        // ──────────────────────────────────────────

        public async Task<UserIndexViewModel> GetUsersAsync(string? search, string filter, int page, int pageSize)
        {
            var adminRole = await _roleManager.FindByNameAsync("Admin");
            var adminIds  = adminRole == null
                ? new HashSet<string>()
                : new HashSet<string>(
                    (await _userManager.GetUsersInRoleAsync("Admin"))
                    .Select(u => u.Id));

            var now    = DateTime.UtcNow;
            var allUsers = await _context.Users.ToListAsync();

            // Order stats per user
            var orderStats = await _context.Orders
                .GroupBy(o => o.UserId)
                .Select(g => new
                {
                    UserId     = g.Key,
                    Count      = g.Count(),
                    TotalSpent = g.Sum(o => o.TotalAmount)
                }).ToListAsync();

            var statDict = orderStats.ToDictionary(s => s.UserId);

            // Active subs
            var activeSubs = await _context.UserSubscriptions
                .Where(s => s.IsActive && s.EndDate >= now)
                .Select(s => s.UserId)
                .ToHashSetAsync();

            // Premium end dates
            var premiumEnds = await _context.UserSubscriptions
                .Where(s => s.IsActive && s.EndDate >= now)
                .GroupBy(s => s.UserId)
                .Select(g => new { UserId = g.Key, EndDate = g.Max(s => s.EndDate) })
                .ToListAsync();
            var premiumEndDict = premiumEnds.ToDictionary(p => p.UserId, p => p.EndDate);

            // Map to VM
            var rows = allUsers.Select(u => new UserListViewModel
            {
                Id           = u.Id,
                FullName     = u.FullName,
                UserName     = u.UserName ?? "",
                Email        = u.Email    ?? "",
                IsAdmin      = adminIds.Contains(u.Id),
                IsPremium    = activeSubs.Contains(u.Id),
                PremiumEndDate = premiumEndDict.TryGetValue(u.Id, out var ed) ? ed : null,
                OrderCount   = statDict.TryGetValue(u.Id, out var s) ? s.Count      : 0,
                TotalSpent   = statDict.TryGetValue(u.Id, out var s2) ? s2.TotalSpent : 0,
                RegisteredAt = u.Email ?? "",
            }).AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(search))
                rows = rows.Where(u =>
                    u.UserName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    u.Email.Contains(search, StringComparison.OrdinalIgnoreCase)    ||
                    u.FullName.Contains(search, StringComparison.OrdinalIgnoreCase));

            // Filter
            rows = filter switch
            {
                "admin"   => rows.Where(u => u.IsAdmin),
                "premium" => rows.Where(u => u.IsPremium),
                "normal"  => rows.Where(u => !u.IsAdmin && !u.IsPremium),
                _         => rows
            };

            var list       = rows.ToList();
            var totalCount = list.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new UserIndexViewModel
            {
                Users      = list.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
                Search     = search,
                Filter     = filter,
                Page       = page,
                TotalPages = totalPages,
                TotalCount = totalCount,
            };
        }

        public async Task<UserDetailViewModel?> GetUserDetailAsync(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return null;

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            var orders  = await _context.Orders
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Plan)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
            var subs = await _subscriptionRepo.GetByUserIdAsync(userId);

            return new UserDetailViewModel
            {
                User        = user,
                IsAdmin     = isAdmin,
                Orders      = orders,
                Subscriptions = subs,
                TotalOrders = orders.Count,
                TotalSpent  = orders.Sum(o => o.TotalAmount),
            };
        }

        public async Task<bool> SetAdminRoleAsync(string userId, bool grantAdmin)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            if (!await _roleManager.RoleExistsAsync("Admin"))
                await _roleManager.CreateAsync(new IdentityRole("Admin"));

            if (grantAdmin)
            {
                if (!await _userManager.IsInRoleAsync(user, "Admin"))
                    await _userManager.AddToRoleAsync(user, "Admin");
            }
            else
            {
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                    await _userManager.RemoveFromRoleAsync(user, "Admin");
            }
            return true;
        }

        public async Task<bool> TogglePremiumAsync(string userId, bool activate, int days = 30)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            // Deactivate all active subs first
            var activeSubs = await _context.UserSubscriptions
                .Where(s => s.UserId == userId && s.IsActive)
                .ToListAsync();
            foreach (var s in activeSubs)
                s.IsActive = false;

            if (activate)
            {
                user.PremiumStartDate = DateTime.UtcNow;
                user.PremiumEndDate   = DateTime.UtcNow.AddDays(days);

                // Find cheapest plan as a placeholder
                var plan = await _context.SubscriptionPlans.OrderBy(p => p.Price).FirstOrDefaultAsync();
                if (plan != null)
                {
                    _context.UserSubscriptions.Add(new UserSubscription
                    {
                        UserId        = userId,
                        PlanId        = plan.Id,
                        StartDate     = DateTime.UtcNow,
                        EndDate       = DateTime.UtcNow.AddDays(days),
                        IsActive      = true,
                        PaymentMethod = "admin_grant",
                        TransactionId = $"ADMIN_{DateTime.UtcNow:yyyyMMddHHmmss}",
                        CreatedAt     = DateTime.UtcNow,
                    });
                }
            }
            else
            {
                user.PremiumStartDate = null;
                user.PremiumEndDate   = null;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // ──────────────────────────────────────────
        // SUBSCRIPTIONS
        // ──────────────────────────────────────────

        public async Task<SubscriptionIndexViewModel> GetSubscriptionsAsync(string? search, string filter, int page, int pageSize)
        {
            var all = (await _subscriptionRepo.GetAllAsync()).ToList();
            var now = DateTime.UtcNow;

            var rows = all.Select(s => new SubscriptionRowViewModel
            {
                Id            = s.Id,
                UserName      = s.User?.UserName ?? "",
                UserEmail     = s.User?.Email    ?? "",
                PlanName      = s.Plan?.Name     ?? "",
                Price         = s.Plan?.Price    ?? 0,
                PaymentMethod = s.PaymentMethod,
                StartDate     = s.StartDate,
                EndDate       = s.EndDate,
                IsActive      = s.IsActive && s.EndDate >= now,
                TransactionId = s.TransactionId,
            }).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                rows = rows.Where(r =>
                    r.UserName.Contains(search, StringComparison.OrdinalIgnoreCase)  ||
                    r.UserEmail.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    r.TransactionId.Contains(search, StringComparison.OrdinalIgnoreCase));

            rows = filter switch
            {
                "active"  => rows.Where(r => r.IsActive),
                "expired" => rows.Where(r => !r.IsActive),
                _         => rows
            };

            var list       = rows.ToList();
            var totalCount = list.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new SubscriptionIndexViewModel
            {
                Subscriptions = list.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
                Search        = search,
                Filter        = filter,
                Page          = page,
                TotalPages    = totalPages,
                TotalCount    = totalCount,
                ActiveCount   = list.Count(r => r.IsActive),
                ExpiredCount  = list.Count(r => !r.IsActive),
                TotalRevenue  = list.Sum(r => r.Price),
            };
        }

        public async Task<bool> DeactivateSubscriptionAsync(int subscriptionId)
        {
            var sub = await _subscriptionRepo.GetByIdAsync(subscriptionId);
            if (sub == null) return false;
            sub.IsActive = false;
            await _subscriptionRepo.UpdateAsync(sub);
            await _subscriptionRepo.SaveAsync();
            return true;
        }

        // ──────────────────────────────────────────
        // ANALYTICS
        // ──────────────────────────────────────────

        public async Task<AnalyticsViewModel> GetAnalyticsAsync()
        {
            var now       = DateTime.UtcNow;
            var startOf12 = new DateTime(now.Year, now.Month, 1).AddMonths(-11);

            var orders = await _context.Orders
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Plan)
                .Where(o => o.CreatedAt >= startOf12)
                .ToListAsync();

            var subs = await _context.UserSubscriptions
                .Include(s => s.Plan)
                .Where(s => s.CreatedAt >= startOf12)
                .ToListAsync();

            var monthlyLabels  = new List<string>();
            var monthlyRevenue = new List<decimal>();
            var monthlyOrders  = new List<int>();
            var monthlyNewSubs = new List<int>();

            for (int i = 11; i >= 0; i--)
            {
                var month = new DateTime(now.Year, now.Month, 1).AddMonths(-i);
                var next  = month.AddMonths(1);
                monthlyLabels.Add(month.ToString("MM/yyyy"));

                var mo = orders.Where(o => o.CreatedAt >= month && o.CreatedAt < next).ToList();
                monthlyRevenue.Add(mo.Where(o => o.Status == OrderStatus.Completed || o.Status == OrderStatus.Paid)
                                     .Sum(o => o.TotalAmount));
                monthlyOrders.Add(mo.Count);
                monthlyNewSubs.Add(subs.Count(s => s.CreatedAt >= month && s.CreatedAt < next));
            }

            // Plan distribution
            var planGroups = subs
                .GroupBy(s => s.Plan?.Name ?? "Unknown")
                .Select(g => new { Name = g.Key, Count = g.Count(), Revenue = g.Sum(s => s.Plan?.Price ?? 0) })
                .OrderByDescending(g => g.Count)
                .ToList();

            // Payment method breakdown
            var paymentGroups = orders
                .GroupBy(o => o.PaymentMethod)
                .Select(g => new { Method = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .ToList();

            var totalUsers  = await _context.Users.CountAsync();
            var premiumUsers = await _context.UserSubscriptions
                .Where(s => s.IsActive && s.EndDate >= now)
                .Select(s => s.UserId).Distinct().CountAsync();

            var totalRevenue = orders
                .Where(o => o.Status == OrderStatus.Completed || o.Status == OrderStatus.Paid)
                .Sum(o => o.TotalAmount);
            var paidOrderCount = orders.Count(o => o.Status == OrderStatus.Completed || o.Status == OrderStatus.Paid);

            return new AnalyticsViewModel
            {
                MonthlyLabels       = monthlyLabels,
                MonthlyRevenue      = monthlyRevenue,
                MonthlyOrders       = monthlyOrders,
                MonthlyNewSubs      = monthlyNewSubs,
                PlanLabels          = planGroups.Select(g => g.Name).ToList(),
                PlanCounts          = planGroups.Select(g => g.Count).ToList(),
                PlanRevenues        = planGroups.Select(g => g.Revenue).ToList(),
                PaymentLabels       = paymentGroups.Select(g => g.Method).ToList(),
                PaymentCounts       = paymentGroups.Select(g => g.Count).ToList(),
                TotalRevenue        = totalRevenue,
                AvgOrderValue       = paidOrderCount == 0 ? 0 : Math.Round(totalRevenue / paidOrderCount, 0),
                TotalOrders         = orders.Count,
                TotalActiveSubscriptions = premiumUsers,
                SubscriptionConversionRate = totalUsers == 0 ? 0 : Math.Round((double)premiumUsers / totalUsers * 100, 1),
            };
        }
    }
}

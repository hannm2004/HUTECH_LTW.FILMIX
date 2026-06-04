using System;
using System.Collections.Generic;
using untitled1.Models.Entities;

namespace untitled1.Models.ViewModels
{
    // ──────────────────────────────────────────
    // DASHBOARD
    // ──────────────────────────────────────────

    public class DashboardViewModel
    {
        // KPI Stats
        public int TotalMovies        { get; set; }
        public int TotalTVSeries      { get; set; }
        public int TotalFilms         { get; set; }
        public int TotalUsers         { get; set; }
        public int TotalOrders        { get; set; }
        public int PendingOrders      { get; set; }
        public int ActiveSubscriptions { get; set; }
        public decimal TotalRevenue   { get; set; }
        public decimal MonthRevenue   { get; set; }

        // Growth deltas (vs previous month, %)
        public double UserGrowthPct   { get; set; }
        public double RevenueGrowthPct { get; set; }

        // Recent activity
        public IEnumerable<Order>            RecentOrders        { get; set; } = new List<Order>();
        public IEnumerable<UserSubscription> RecentSubscriptions { get; set; } = new List<UserSubscription>();
        public IEnumerable<Movie>            RecentMovies        { get; set; } = new List<Movie>();

        // Chart data (JSON-serializable)
        public List<string> RevenueChartLabels { get; set; } = new();
        public List<decimal> RevenueChartData  { get; set; } = new();
        public List<string>  UserChartLabels   { get; set; } = new();
        public List<int>     UserChartData     { get; set; } = new();
    }

    // ──────────────────────────────────────────
    // USER MANAGEMENT
    // ──────────────────────────────────────────

    public class UserListViewModel
    {
        public string   Id            { get; set; } = string.Empty;
        public string   FullName      { get; set; } = string.Empty;
        public string   UserName      { get; set; } = string.Empty;
        public string   Email         { get; set; } = string.Empty;
        public bool     IsAdmin       { get; set; }
        public bool     IsPremium     { get; set; }
        public DateTime? PremiumEndDate { get; set; }
        public int      OrderCount    { get; set; }
        public decimal  TotalSpent    { get; set; }
        public string   RegisteredAt  { get; set; } = string.Empty;  // formatted
    }

    public class UserDetailViewModel
    {
        public ApplicationUser     User          { get; set; } = null!;
        public bool                IsAdmin       { get; set; }
        public IEnumerable<Order>  Orders        { get; set; } = new List<Order>();
        public IEnumerable<UserSubscription> Subscriptions { get; set; } = new List<UserSubscription>();
        public decimal             TotalSpent    { get; set; }
        public int                 TotalOrders   { get; set; }
    }

    public class UserIndexViewModel
    {
        public IEnumerable<UserListViewModel> Users     { get; set; } = new List<UserListViewModel>();
        public string?   Search      { get; set; }
        public string    Filter      { get; set; } = "all";   // all | admin | premium | normal
        public int       Page        { get; set; } = 1;
        public int       TotalPages  { get; set; }
        public int       TotalCount  { get; set; }
    }

    // ──────────────────────────────────────────
    // SUBSCRIPTION MANAGEMENT
    // ──────────────────────────────────────────

    public class SubscriptionRowViewModel
    {
        public int      Id            { get; set; }
        public string   UserName      { get; set; } = string.Empty;
        public string   UserEmail     { get; set; } = string.Empty;
        public string   PlanName      { get; set; } = string.Empty;
        public decimal  Price         { get; set; }
        public string   PaymentMethod { get; set; } = string.Empty;
        public DateTime StartDate     { get; set; }
        public DateTime EndDate       { get; set; }
        public bool     IsActive      { get; set; }
        public string   TransactionId { get; set; } = string.Empty;
    }

    public class SubscriptionIndexViewModel
    {
        public IEnumerable<SubscriptionRowViewModel> Subscriptions { get; set; } = new List<SubscriptionRowViewModel>();
        public string? Search      { get; set; }
        public string  Filter      { get; set; } = "all";   // all | active | expired
        public int     Page        { get; set; } = 1;
        public int     TotalPages  { get; set; }
        public int     TotalCount  { get; set; }

        // Summary stats
        public int     ActiveCount  { get; set; }
        public int     ExpiredCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    // ──────────────────────────────────────────
    // ANALYTICS
    // ──────────────────────────────────────────

    public class AnalyticsViewModel
    {
        // Revenue chart — last 12 months
        public List<string>  MonthlyLabels        { get; set; } = new();
        public List<decimal> MonthlyRevenue        { get; set; } = new();
        public List<int>     MonthlyOrders         { get; set; } = new();
        public List<int>     MonthlyNewUsers       { get; set; } = new();
        public List<int>     MonthlyNewSubs        { get; set; } = new();

        // Plan distribution (for pie/doughnut chart)
        public List<string> PlanLabels            { get; set; } = new();
        public List<int>    PlanCounts            { get; set; } = new();
        public List<decimal> PlanRevenues         { get; set; } = new();

        // Payment method breakdown
        public List<string> PaymentLabels         { get; set; } = new();
        public List<int>    PaymentCounts         { get; set; } = new();

        // Summary KPIs
        public decimal TotalRevenue               { get; set; }
        public decimal AvgOrderValue              { get; set; }
        public int     TotalOrders                { get; set; }
        public int     TotalActiveSubscriptions   { get; set; }
        public double  SubscriptionConversionRate { get; set; }  // premium users / total users %

        // Top Viewing History Stats
        public List<CategoryWatchStatDto> TopGenres { get; set; } = new();
        public List<MovieWatchStatDto> TopMovies    { get; set; } = new();
    }

    public class CategoryWatchStatDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public int WatchCount      { get; set; }
    }

    public class MovieWatchStatDto
    {
        public Movie Movie    { get; set; } = null!;
        public int WatchCount { get; set; }
    }
}

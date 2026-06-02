using System.Collections.Generic;
using System.Threading.Tasks;
using untitled1.Models.ViewModels;

namespace untitled1.Services
{
    public interface IAdminService
    {
        // Dashboard
        Task<DashboardViewModel> GetDashboardDataAsync();

        // Users
        Task<UserIndexViewModel> GetUsersAsync(string? search, string filter, int page, int pageSize);
        Task<UserDetailViewModel?> GetUserDetailAsync(string userId);
        Task<bool> SetAdminRoleAsync(string userId, bool grantAdmin);
        Task<bool> TogglePremiumAsync(string userId, bool activate, int days = 30);

        // Subscriptions
        Task<SubscriptionIndexViewModel> GetSubscriptionsAsync(string? search, string filter, int page, int pageSize);
        Task<bool> DeactivateSubscriptionAsync(int subscriptionId);

        // Analytics
        Task<AnalyticsViewModel> GetAnalyticsAsync();
    }
}

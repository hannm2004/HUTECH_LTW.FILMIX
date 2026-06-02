using System.Collections.Generic;
using System.Threading.Tasks;
using untitled1.Models.Entities;

namespace untitled1.Repositories
{
    public interface ISubscriptionRepository
    {
        Task<IEnumerable<UserSubscription>> GetAllAsync();
        Task<IEnumerable<UserSubscription>> GetByUserIdAsync(string userId);
        Task<UserSubscription?> GetByIdAsync(int id);
        Task<UserSubscription?> GetActiveByUserIdAsync(string userId);
        Task UpdateAsync(UserSubscription subscription);
        Task SaveAsync();
    }
}

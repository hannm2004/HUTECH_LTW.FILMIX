using System.Collections.Generic;
using System.Threading.Tasks;
using untitled1.Models.Entities;

namespace untitled1.Repositories
{
    public interface ISubscriptionPlanRepository
    {
        Task<SubscriptionPlan?> GetByIdAsync(int id);
        Task<IEnumerable<SubscriptionPlan>> GetAllAsync();
    }
}

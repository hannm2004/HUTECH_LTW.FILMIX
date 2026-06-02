using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using untitled1.Data;
using untitled1.Models.Entities;

namespace untitled1.Repositories
{
    public class SubscriptionPlanRepository : ISubscriptionPlanRepository
    {
        private readonly ApplicationDbContext _context;

        public SubscriptionPlanRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SubscriptionPlan?> GetByIdAsync(int id)
        {
            return await _context.SubscriptionPlans.FindAsync(id);
        }

        public async Task<IEnumerable<SubscriptionPlan>> GetAllAsync()
        {
            return await _context.SubscriptionPlans.OrderBy(p => p.Price).ToListAsync();
        }
    }
}

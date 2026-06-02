using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using untitled1.Data;
using untitled1.Models.Entities;

namespace untitled1.Repositories
{
    public class SubscriptionRepository : ISubscriptionRepository
    {
        private readonly ApplicationDbContext _context;

        public SubscriptionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UserSubscription>> GetAllAsync()
        {
            return await _context.UserSubscriptions
                .Include(s => s.User)
                .Include(s => s.Plan)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserSubscription>> GetByUserIdAsync(string userId)
        {
            return await _context.UserSubscriptions
                .Include(s => s.Plan)
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<UserSubscription?> GetByIdAsync(int id)
        {
            return await _context.UserSubscriptions
                .Include(s => s.User)
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<UserSubscription?> GetActiveByUserIdAsync(string userId)
        {
            return await _context.UserSubscriptions
                .Include(s => s.Plan)
                .Where(s => s.UserId == userId && s.IsActive && s.EndDate >= DateTime.UtcNow)
                .OrderByDescending(s => s.EndDate)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(UserSubscription subscription)
        {
            _context.UserSubscriptions.Update(subscription);
            await Task.CompletedTask;
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}

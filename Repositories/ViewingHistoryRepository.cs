using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using untitled1.Data;
using untitled1.Models.Entities;

namespace untitled1.Repositories
{
    public class ViewingHistoryRepository : IViewingHistoryRepository
    {
        private readonly ApplicationDbContext _context;

        public ViewingHistoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(ViewingHistory history)
        {
            await _context.ViewingHistories.AddAsync(history);
        }

        public async Task<IEnumerable<ViewingHistory>> GetByUserIdAsync(string userId)
        {
            return await _context.ViewingHistories
                .Include(vh => vh.Movie)
                    .ThenInclude(m => m.MovieCategories)
                        .ThenInclude(mc => mc.Category)
                .Where(vh => vh.UserId == userId)
                .OrderByDescending(vh => vh.WatchedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ViewingHistory>> GetAllAsync()
        {
            return await _context.ViewingHistories
                .Include(vh => vh.Movie)
                    .ThenInclude(m => m.MovieCategories)
                        .ThenInclude(mc => mc.Category)
                .ToListAsync();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using untitled1.Models.Entities;

namespace untitled1.Repositories
{
    public interface IViewingHistoryRepository
    {
        Task AddAsync(ViewingHistory history);
        Task<IEnumerable<ViewingHistory>> GetByUserIdAsync(string userId);
        Task<IEnumerable<ViewingHistory>> GetAllAsync();
        Task SaveAsync();
    }
}

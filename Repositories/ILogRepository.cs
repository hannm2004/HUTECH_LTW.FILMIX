using System.Collections.Generic;
using System.Threading.Tasks;
using untitled1.Models.Entities;

namespace untitled1.Repositories
{
    public interface ILogRepository
    {
        Task AddAsync(SystemLog log);
        Task<SystemLog?> GetByIdAsync(int id);
        Task<IEnumerable<SystemLog>> GetAllAsync(string? search, string? actionType, int page, int pageSize);
        Task<int> GetTotalCountAsync(string? search, string? actionType);
        Task<List<string>> GetActionTypesAsync();
        Task SaveAsync();
    }
}

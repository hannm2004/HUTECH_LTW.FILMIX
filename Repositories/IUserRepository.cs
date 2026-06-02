using System.Collections.Generic;
using System.Threading.Tasks;
using untitled1.Models.Entities;

namespace untitled1.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<ApplicationUser>> GetAllAsync();
        Task<ApplicationUser?> GetByIdAsync(string userId);
        Task UpdateAsync(ApplicationUser user);
        Task SaveAsync();
    }
}

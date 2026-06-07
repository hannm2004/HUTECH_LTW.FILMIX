using System.Threading.Tasks;
using untitled1.Models.Entities;

namespace untitled1.Services
{
    public interface IJwtService
    {
        Task<string> GenerateAccessToken(ApplicationUser user);
    }
}

using System.Threading.Tasks;
using untitled1.Models.Entities;
using untitled1.Models.ViewModels;

namespace untitled1.Services
{
    public interface ILogService
    {
        Task LogAsync(string? userId, string? userName, string action, string description, string? ipAddress);
        Task<SystemLog?> GetLogDetailAsync(int id);
        Task<SystemLogIndexViewModel> GetLogsAsync(string? search, string? actionType, int page, int pageSize);
    }
}

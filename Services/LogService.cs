using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using untitled1.Models.Entities;
using untitled1.Models.ViewModels;
using untitled1.Repositories;

namespace untitled1.Services
{
    public class LogService : ILogService
    {
        private readonly ILogRepository _logRepo;

        public LogService(ILogRepository logRepo)
        {
            _logRepo = logRepo;
        }

        public async Task LogAsync(string? userId, string? userName, string action, string description, string? ipAddress)
        {
            var log = new SystemLog
            {
                UserId = userId,
                UserName = userName ?? "Anonymous",
                Action = action,
                Description = description,
                IpAddress = ipAddress,
                CreatedAt = DateTime.Now
            };

            await _logRepo.AddAsync(log);
            await _logRepo.SaveAsync();
        }

        public async Task<SystemLog?> GetLogDetailAsync(int id)
        {
            return await _logRepo.GetByIdAsync(id);
        }

        public async Task<SystemLogIndexViewModel> GetLogsAsync(string? search, string? actionType, int page, int pageSize)
        {
            var logs = await _logRepo.GetAllAsync(search, actionType, page, pageSize);
            var totalCount = await _logRepo.GetTotalCountAsync(search, actionType);
            var actionTypes = await _logRepo.GetActionTypesAsync();

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new SystemLogIndexViewModel
            {
                Logs = logs,
                Search = search,
                ActionFilter = actionType ?? "all",
                ActionTypes = actionTypes,
                Page = page,
                TotalPages = totalPages,
                TotalCount = totalCount
            };
        }
    }
}

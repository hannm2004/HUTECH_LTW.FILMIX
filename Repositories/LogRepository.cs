using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using untitled1.Data;
using untitled1.Models.Entities;

namespace untitled1.Repositories
{
    public class LogRepository : ILogRepository
    {
        private readonly ApplicationDbContext _context;

        public LogRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(SystemLog log)
        {
            await _context.SystemLogs.AddAsync(log);
        }

        public async Task<SystemLog?> GetByIdAsync(int id)
        {
            return await _context.SystemLogs.FindAsync(id);
        }

        public async Task<IEnumerable<SystemLog>> GetAllAsync(string? search, string? actionType, int page, int pageSize)
        {
            var query = _context.SystemLogs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(l => 
                    (l.UserName != null && l.UserName.Contains(search)) || 
                    (l.Description != null && l.Description.Contains(search)) ||
                    (l.IpAddress != null && l.IpAddress.Contains(search)) ||
                    l.Action.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(actionType) && actionType != "all")
            {
                query = query.Where(l => l.Action == actionType);
            }

            return await query
                .OrderByDescending(l => l.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync(string? search, string? actionType)
        {
            var query = _context.SystemLogs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(l => 
                    (l.UserName != null && l.UserName.Contains(search)) || 
                    (l.Description != null && l.Description.Contains(search)) ||
                    (l.IpAddress != null && l.IpAddress.Contains(search)) ||
                    l.Action.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(actionType) && actionType != "all")
            {
                query = query.Where(l => l.Action == actionType);
            }

            return await query.CountAsync();
        }

        public async Task<List<string>> GetActionTypesAsync()
        {
            return await _context.SystemLogs
                .Select(l => l.Action)
                .Distinct()
                .OrderBy(a => a)
                .ToListAsync();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}

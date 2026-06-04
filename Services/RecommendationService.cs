using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using untitled1.Data;
using untitled1.Models.Entities;
using untitled1.Models.ViewModels;
using untitled1.Repositories;

namespace untitled1.Services
{
    public class RecommendationService : IRecommendationService
    {
        private readonly IViewingHistoryRepository _viewingHistoryRepo;
        private readonly ApplicationDbContext _context;

        public RecommendationService(IViewingHistoryRepository viewingHistoryRepo, ApplicationDbContext context)
        {
            _viewingHistoryRepo = viewingHistoryRepo;
            _context = context;
        }

        public async Task LogWatchHistoryAsync(string? userId, int movieId, int watchTime)
        {
            if (string.IsNullOrEmpty(userId) || movieId <= 0) return;

            var historyList = await _viewingHistoryRepo.GetByUserIdAsync(userId);
            var existing = historyList.FirstOrDefault(vh => vh.MovieId == movieId);

            if (existing != null)
            {
                existing.WatchTime = watchTime;
                existing.WatchedAt = DateTime.Now;
            }
            else
            {
                var history = new ViewingHistory
                {
                    UserId = userId,
                    MovieId = movieId,
                    WatchTime = watchTime,
                    WatchedAt = DateTime.Now
                };
                await _viewingHistoryRepo.AddAsync(history);
            }
            await _viewingHistoryRepo.SaveAsync();
        }

        public async Task<IEnumerable<Movie>> GetRecommendationsAsync(string? userId, int count = 10)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return await GetDefaultRecommendationsAsync(count);
            }

            var history = (await _viewingHistoryRepo.GetByUserIdAsync(userId)).ToList();
            var watchedMovieIds = history.Select(h => h.MovieId).Distinct().ToList();

            if (!history.Any())
            {
                return await GetDefaultRecommendationsAsync(count);
            }

            // Count watch frequency of each category (genre) from user's history
            var categoryCounts = history
                .Select(h => h.Movie)
                .Where(m => m != null)
                .SelectMany(m => m.MovieCategories)
                .GroupBy(mc => mc.CategoryId)
                .Select(g => new { CategoryId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            if (!categoryCounts.Any())
            {
                // Fallback: If no categories resolved, return newest unwatched movies
                return await _context.Movies
                    .Include(m => m.MovieCategories)
                        .ThenInclude(mc => mc.Category)
                    .Where(m => !watchedMovieIds.Contains(m.Id))
                    .OrderByDescending(m => m.Year)
                    .ThenByDescending(m => m.Id)
                    .Take(count)
                    .ToListAsync();
            }

            // Get the user's top category IDs (up to 3 to ensure diversity in suggestions)
            var topCategoryIds = categoryCounts.Take(3).Select(x => x.CategoryId).ToList();

            // Find movies in those top categories that the user has not watched yet
            var recommendations = await _context.Movies
                .Include(m => m.MovieCategories)
                    .ThenInclude(mc => mc.Category)
                .Where(m => !watchedMovieIds.Contains(m.Id) && m.MovieCategories.Any(mc => topCategoryIds.Contains(mc.CategoryId)))
                .OrderByDescending(m => m.Year)
                .ThenByDescending(m => m.Id)
                .Take(count)
                .ToListAsync();

            // If we don't have enough recommendations, pad the list with other unwatched movies
            if (recommendations.Count < count)
            {
                var existingRecIds = recommendations.Select(r => r.Id).ToList();
                var remainingCount = count - recommendations.Count;
                var fallbackMovies = await _context.Movies
                    .Include(m => m.MovieCategories)
                        .ThenInclude(mc => mc.Category)
                    .Where(m => !watchedMovieIds.Contains(m.Id) && !existingRecIds.Contains(m.Id))
                    .OrderByDescending(m => m.Year)
                    .ThenByDescending(m => m.Id)
                    .Take(remainingCount)
                    .ToListAsync();

                recommendations.AddRange(fallbackMovies);
            }

            return recommendations;
        }

        public async Task<IEnumerable<CategoryWatchStatDto>> GetTopGenresAsync(int count = 10)
        {
            var allHistory = await _viewingHistoryRepo.GetAllAsync();

            var stats = allHistory
                .Select(h => h.Movie)
                .Where(m => m != null)
                .SelectMany(m => m.MovieCategories)
                .GroupBy(mc => mc.Category.Name)
                .Select(g => new CategoryWatchStatDto
                {
                    CategoryName = g.Key,
                    WatchCount = g.Count()
                })
                .OrderByDescending(x => x.WatchCount)
                .Take(count)
                .ToList();

            return stats;
        }

        public async Task<IEnumerable<MovieWatchStatDto>> GetTopMoviesAsync(int count = 10)
        {
            var allHistory = await _viewingHistoryRepo.GetAllAsync();

            var stats = allHistory
                .GroupBy(h => h.MovieId)
                .Select(g => new
                {
                    MovieId = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(count)
                .ToList();

            var movieIds = stats.Select(s => s.MovieId).ToList();
            var movies = await _context.Movies
                .Include(m => m.MovieCategories)
                    .ThenInclude(mc => mc.Category)
                .Where(m => movieIds.Contains(m.Id))
                .ToListAsync();

            var result = stats
                .Select(s => new MovieWatchStatDto
                {
                    Movie = movies.FirstOrDefault(m => m.Id == s.MovieId)!,
                    WatchCount = s.Count
                })
                .Where(r => r.Movie != null)
                .ToList();

            return result;
        }

        private async Task<IEnumerable<Movie>> GetDefaultRecommendationsAsync(int count)
        {
            return await _context.Movies
                .Include(m => m.MovieCategories)
                    .ThenInclude(mc => mc.Category)
                .OrderByDescending(m => m.Year)
                .ThenByDescending(m => m.Id)
                .Take(count)
                .ToListAsync();
        }
    }
}

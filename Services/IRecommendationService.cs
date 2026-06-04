using System.Collections.Generic;
using System.Threading.Tasks;
using untitled1.Models.Entities;
using untitled1.Models.ViewModels;

namespace untitled1.Services
{
    public interface IRecommendationService
    {
        Task LogWatchHistoryAsync(string? userId, int movieId, int watchTime);
        Task<IEnumerable<Movie>> GetRecommendationsAsync(string? userId, int count = 10);
        Task<IEnumerable<CategoryWatchStatDto>> GetTopGenresAsync(int count = 10);
        Task<IEnumerable<MovieWatchStatDto>> GetTopMoviesAsync(int count = 10);
    }
}

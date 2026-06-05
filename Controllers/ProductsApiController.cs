using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using untitled1.Data;
using untitled1.Models.DTOs;
using untitled1.Models.Entities;
using untitled1.Services;

namespace untitled1.Controllers
{
    [ApiController]
    [Route("api/products")]
    [Authorize(Roles = "Admin")]
    public class ProductsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogService _logService;

        public ProductsApiController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogService logService)
        {
            _context = context;
            _userManager = userManager;
            _logService = logService;
        }

        // GET: api/products
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _context.Movies
                .Include(m => m.MovieCategories)
                    .ThenInclude(mc => mc.Category)
                .AsSplitQuery()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(m => m.Title.Contains(search));
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var movies = await query
                .OrderBy(m => m.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var listDto = movies.Select(m => new MovieListDto
            {
                Id = m.Id,
                Title = m.Title,
                ImageUrl = m.ImageUrl,
                Year = m.Year,
                Genre = m.Genre,
                IsTVSeries = m.IsTVSeries,
                Categories = m.MovieCategories.Select(mc => mc.Category.Name).ToList()
            }).ToList();

            var result = new
            {
                Items = listDto,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalCount = totalCount
            };

            return Ok(ApiResponse<object>.SuccessResponse(result, "Lấy danh sách phim thành công."));
        }

        // GET: api/products/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var movie = await _context.Movies
                .Include(m => m.MovieCategories)
                    .ThenInclude(mc => mc.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse($"Không tìm thấy phim có mã ID {id}."));
            }

            var detailDto = new MovieDetailDto
            {
                Id = movie.Id,
                Title = movie.Title,
                ImageUrl = movie.ImageUrl,
                Year = movie.Year,
                Genre = movie.Genre,
                IsTVSeries = movie.IsTVSeries,
                Director = movie.Director,
                Cast = movie.Cast,
                Description = movie.Description,
                TrailerUrl = movie.TrailerUrl,
                CategoryIds = movie.MovieCategories.Select(mc => mc.CategoryId).ToList(),
                Categories = movie.MovieCategories.Select(mc => mc.Category.Name).ToList()
            };

            return Ok(ApiResponse<MovieDetailDto>.SuccessResponse(detailDto, "Lấy chi tiết phim thành công."));
        }

        // POST: api/products
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateMovieDto dto)
        {
            var movie = new Movie
            {
                Title = dto.Title,
                ImageUrl = dto.ImageUrl,
                Year = dto.Year,
                Genre = dto.Genre,
                IsTVSeries = dto.IsTVSeries,
                Director = dto.Director ?? string.Empty,
                Cast = dto.Cast ?? string.Empty,
                Description = dto.Description ?? string.Empty,
                TrailerUrl = dto.TrailerUrl ?? string.Empty
            };

            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();

            if (dto.CategoryIds != null && dto.CategoryIds.Any())
            {
                foreach (var catId in dto.CategoryIds)
                {
                    _context.MovieCategories.Add(new MovieCategory { MovieId = movie.Id, CategoryId = catId });
                }
                await _context.SaveChangesAsync();
            }

            // Retrieve populated entity for Response
            var createdMovie = await _context.Movies
                .Include(m => m.MovieCategories)
                    .ThenInclude(mc => mc.Category)
                .FirstAsync(m => m.Id == movie.Id);

            var detailDto = new MovieDetailDto
            {
                Id = createdMovie.Id,
                Title = createdMovie.Title,
                ImageUrl = createdMovie.ImageUrl,
                Year = createdMovie.Year,
                Genre = createdMovie.Genre,
                IsTVSeries = createdMovie.IsTVSeries,
                Director = createdMovie.Director,
                Cast = createdMovie.Cast,
                Description = createdMovie.Description,
                TrailerUrl = createdMovie.TrailerUrl,
                CategoryIds = createdMovie.MovieCategories.Select(mc => mc.CategoryId).ToList(),
                Categories = createdMovie.MovieCategories.Select(mc => mc.Category.Name).ToList()
            };

            var admin = await _userManager.GetUserAsync(User);
            await _logService.LogAsync(
                admin?.Id,
                admin?.Email,
                "Add Movie",
                $"Thêm phim mới qua API: \"{movie.Title}\" (ID: {movie.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString()
            );

            return CreatedAtAction(nameof(GetById), new { id = movie.Id }, ApiResponse<MovieDetailDto>.SuccessResponse(detailDto, "Thêm phim mới thành công."));
        }

        // PUT: api/products/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateMovieDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Mã phim trên URL không khớp với dữ liệu gửi lên."));
            }

            var movie = await _context.Movies
                .Include(m => m.MovieCategories)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse($"Không tìm thấy phim có mã ID {id} để cập nhật."));
            }

            movie.Title = dto.Title;
            movie.ImageUrl = dto.ImageUrl;
            movie.Year = dto.Year;
            movie.Genre = dto.Genre;
            movie.IsTVSeries = dto.IsTVSeries;
            movie.Director = dto.Director ?? string.Empty;
            movie.Cast = dto.Cast ?? string.Empty;
            movie.Description = dto.Description ?? string.Empty;
            movie.TrailerUrl = dto.TrailerUrl ?? string.Empty;

            // Update Categories
            var existingCategories = _context.MovieCategories.Where(mc => mc.MovieId == id);
            _context.MovieCategories.RemoveRange(existingCategories);

            if (dto.CategoryIds != null && dto.CategoryIds.Any())
            {
                foreach (var catId in dto.CategoryIds)
                {
                    _context.MovieCategories.Add(new MovieCategory { MovieId = id, CategoryId = catId });
                }
            }

            await _context.SaveChangesAsync();

            // Retrieve updated detail
            var updatedMovie = await _context.Movies
                .Include(m => m.MovieCategories)
                    .ThenInclude(mc => mc.Category)
                .FirstAsync(m => m.Id == id);

            var detailDto = new MovieDetailDto
            {
                Id = updatedMovie.Id,
                Title = updatedMovie.Title,
                ImageUrl = updatedMovie.ImageUrl,
                Year = updatedMovie.Year,
                Genre = updatedMovie.Genre,
                IsTVSeries = updatedMovie.IsTVSeries,
                Director = updatedMovie.Director,
                Cast = updatedMovie.Cast,
                Description = updatedMovie.Description,
                TrailerUrl = updatedMovie.TrailerUrl,
                CategoryIds = updatedMovie.MovieCategories.Select(mc => mc.CategoryId).ToList(),
                Categories = updatedMovie.MovieCategories.Select(mc => mc.Category.Name).ToList()
            };

            var admin = await _userManager.GetUserAsync(User);
            await _logService.LogAsync(
                admin?.Id,
                admin?.Email,
                "Edit Movie",
                $"Sửa thông tin phim qua API: \"{movie.Title}\" (ID: {movie.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString()
            );

            return Ok(ApiResponse<MovieDetailDto>.SuccessResponse(detailDto, "Cập nhật thông tin phim thành công."));
        }

        // DELETE: api/products/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var movie = await _context.Movies
                .Include(m => m.MovieCategories)
                .Include(m => m.Episodes)
                .Include(m => m.MovieImages)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse($"Không tìm thấy phim có mã ID {id} để xóa."));
            }

            var title = movie.Title;
            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();

            var admin = await _userManager.GetUserAsync(User);
            await _logService.LogAsync(
                admin?.Id,
                admin?.Email,
                "Delete Movie",
                $"Xóa phim qua API: \"{title}\" (ID: {id})",
                HttpContext.Connection.RemoteIpAddress?.ToString()
            );

            return Ok(ApiResponse<object>.SuccessResponse(null, $"Xóa phim \"{title}\" thành công."));
        }
    }
}

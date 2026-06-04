using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using untitled1.Data;
using untitled1.Models.ViewModels;
using untitled1.Models.Entities;
using untitled1.Services;

namespace untitled1.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;
    private readonly IRecommendationService _recommendationService;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(
        ILogger<HomeController> logger, 
        ApplicationDbContext context, 
        IRecommendationService recommendationService,
        UserManager<ApplicationUser> userManager)
    {
        _logger = logger;
        _context = context;
        _recommendationService = recommendationService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        // Top 10 trending (newest by year, all types)
        var trending = await _context.Movies
            .OrderByDescending(m => m.Year)
            .ThenByDescending(m => m.Id)
            .Take(10)
            .ToListAsync();

        // Newest 20 for the "See All" expanded grid
        var allMovies = await _context.Movies
            .OrderByDescending(m => m.Year)
            .ThenByDescending(m => m.Id)
            .Take(20)
            .ToListAsync();

        ViewBag.Trending   = trending;
        ViewBag.AllMovies  = allMovies;

        // Fetch Recommendations for current user
        var userId = _userManager.GetUserId(User);
        var recommendations = await _recommendationService.GetRecommendationsAsync(userId, 10);
        ViewBag.Recommendations = recommendations;

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

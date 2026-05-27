using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using untitled1.Data;
using untitled1.Models.ViewModels;

namespace untitled1.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
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

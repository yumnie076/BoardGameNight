using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BoardGameNight.WebApp.Models;
using BoardGameNight.Application.Interfaces;

namespace BoardGameNight.WebApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IGameNightService _gameNightService;

    public HomeController(ILogger<HomeController> logger, IGameNightService gameNightService)
    {
        _logger = logger;
        _gameNightService = gameNightService;
    }

    public async Task<IActionResult> Index()
    {
        var upcomingNights = await _gameNightService.GetAllUpcomingAsync();
        return View(upcomingNights);
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

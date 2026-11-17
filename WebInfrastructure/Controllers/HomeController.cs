using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebInfrastructure.Services;

namespace WebInfrastructure.Controllers;

/// <summary>The home page with the best sellers, and the informational pages.</summary>
public class HomeController : Controller
{
    private const int BestSellerCount = 6;

    private readonly IInventoryService _inventory;

    public HomeController(IInventoryService inventory)
    {
        _inventory = inventory;
    }

    public IActionResult Index()
    {
        return View(_inventory.GetBestSellers(BestSellerCount));
    }

    public IActionResult About()
    {
        ViewBag.Message = "Your application description page.";
        return View();
    }

    public IActionResult Contact()
    {
        ViewBag.Message = "Your contact page.";
        return View();
    }

    /// <summary>The page the exception handler shows when a request fails (for example when the database is unavailable).</summary>
    public IActionResult Error()
    {
        Response.StatusCode = StatusCodes.Status500InternalServerError;
        return View();
    }
}

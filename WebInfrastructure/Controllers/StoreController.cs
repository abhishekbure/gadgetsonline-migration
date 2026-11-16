using Microsoft.AspNetCore.Mvc;
using WebInfrastructure.Services;

namespace WebInfrastructure.Controllers;

/// <summary>Browsing the catalogue: the store landing page, products of a category, product details.</summary>
public class StoreController : Controller
{
    private readonly IInventoryService _inventory;
    private readonly ILogger<StoreController> _logger;

    public StoreController(IInventoryService inventory, ILogger<StoreController> logger)
    {
        _inventory = inventory;
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View(_inventory.GetAllCategories());
    }

    public IActionResult Browse(string? category)
    {
        var products = _inventory.GetAllProductsInCategory(category);
        if (products.Count == 0 && !string.IsNullOrEmpty(category))
        {
            _logger.LogInformation("web_infrastructure.access.invalid category {Category}", Printable(category));
        }

        return View(products);
    }

    public IActionResult Details(int id)
    {
        var product = _inventory.GetProductById(id);
        if (product is null)
        {
            _logger.LogWarning("web_infrastructure.access.invalid product {ProductId}", id);
            return NotFound();
        }

        return View(product);
    }

    /// <summary>Request text for the log: one line, no control characters, at most 50 characters.</summary>
    private static string Printable(string text)
    {
        var line = new string(text.Select(c => char.IsControl(c) ? ' ' : c).ToArray());
        return line.Length <= 50 ? line : line[..50];
    }
}

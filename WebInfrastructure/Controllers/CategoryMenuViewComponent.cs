using Microsoft.AspNetCore.Mvc;
using WebInfrastructure.Services;

namespace WebInfrastructure.Controllers;

/// <summary>The category menu shown on every page; it cannot be requested on its own.</summary>
public class CategoryMenuViewComponent : ViewComponent
{
    private readonly IInventoryService _inventory;

    public CategoryMenuViewComponent(IInventoryService inventory)
    {
        _inventory = inventory;
    }

    public IViewComponentResult Invoke()
    {
        return View(_inventory.GetAllCategories());
    }
}

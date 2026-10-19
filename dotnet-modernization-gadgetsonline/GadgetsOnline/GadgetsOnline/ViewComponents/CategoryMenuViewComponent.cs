using Microsoft.AspNetCore.Mvc;
using GadgetsOnline.Services;

namespace GadgetsOnline.ViewComponents
{
    public class CategoryMenuViewComponent : ViewComponent
    {
        private readonly Inventory _inventory;
        public CategoryMenuViewComponent(Inventory inventory) { _inventory = inventory; }
        public IViewComponentResult Invoke()
            => View(_inventory.GetAllCategories());
    }
}

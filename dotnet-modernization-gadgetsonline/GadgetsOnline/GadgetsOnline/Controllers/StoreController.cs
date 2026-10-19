using Microsoft.AspNetCore.Mvc;
using GadgetsOnline.Services;

namespace GadgetsOnline.Controllers
{
    public class StoreController : Controller
    {
        private readonly Inventory _inventory;

        public StoreController(Inventory inventory)
        {
            _inventory = inventory;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Browse(string category)
        {
            ViewBag.Category = category;
            var productModel = _inventory.GetAllProductsInCategory(category);
            return View(productModel);
        }

        public ActionResult Details(int id)
        {
            var album = _inventory.GetProductById(id);
            if (album == null) return NotFound();
            return View(album);
        }
    }
}

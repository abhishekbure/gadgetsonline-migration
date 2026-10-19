using Microsoft.AspNetCore.Mvc;
using GadgetsOnline.Services;
using GadgetsOnline.ViewModel;

namespace GadgetsOnline.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly ShoppingCart _cart;
        private readonly Inventory _inventory;

        public ShoppingCartController(ShoppingCart cart, Inventory inventory)
        {
            _cart = cart;
            _inventory = inventory;
        }

        public ActionResult Index()
        {
            var viewModel = new ShoppingCartViewModel
            {
                CartItems = _cart.GetCartItems(),
                CartTotal = _cart.GetTotal()
            };
            return View(viewModel);
        }

        public ActionResult AddToCart(int id)
        {
            _cart.AddToCart(id);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult RemoveFromCart(int id)
        {
            int itemCount = _cart.RemoveFromCart(id);
            var productName = _inventory.GetProductNameById(id);

            var results = new ShoppingCartRemoveViewModel
            {
                Message = System.Net.WebUtility.HtmlEncode(productName) + " has been removed from your shopping cart.",
                CartTotal = _cart.GetTotal(),
                CartCount = _cart.GetCount(),
                ItemCount = itemCount,
                DeleteId = id
            };
            return Json(results);
        }
    }
}

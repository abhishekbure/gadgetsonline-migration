using GadgetsOnline.Services;
using Microsoft.AspNetCore.Mvc;

namespace GadgetsOnline.Controllers
{
    public class HomeController : Controller
    {
        private readonly Inventory _inventory;

        public HomeController(Inventory inventory)
        {
            _inventory = inventory;
        }

        public ActionResult Index()
        {
            var products = _inventory.GetBestSellers(6);
            return View(products);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }

        public ActionResult Error(int? statusCode)
        {
            ViewBag.StatusCode = statusCode ?? 500;
            ViewBag.Message = statusCode switch
            {
                404 => "The page you are looking for could not be found.",
                403 => "You do not have permission to view this page.",
                _ => "Something went wrong. Please try again."
            };
            return View();
        }
    }
}

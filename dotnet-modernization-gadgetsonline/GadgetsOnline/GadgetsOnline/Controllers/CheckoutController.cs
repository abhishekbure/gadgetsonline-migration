using System;
using Microsoft.AspNetCore.Mvc;
using GadgetsOnline.Models;
using GadgetsOnline.Services;

namespace GadgetsOnline.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly OrderProcessing _orderProcessing;
        private readonly ShoppingCart _cart;

        public CheckoutController(OrderProcessing orderProcessing, ShoppingCart cart)
        {
            _orderProcessing = orderProcessing;
            _cart = cart;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AddressAndPayment()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddressAndPayment(Order order)
        {
            if (!ModelState.IsValid) return View(order);

            if (_cart.GetCartItems().Count == 0)
            {
                TempData["Message"] = "Your cart is empty. Please add items before checking out.";
                return RedirectToAction("Index", "ShoppingCart");
            }

            try
            {
                order.Username = "Anonymous";
                order.OrderDate = DateTime.Now;

                bool result = _orderProcessing.ProcessOrder(order);

                return RedirectToAction("Complete",
                    new { id = order.OrderId });
            }
            catch
            {
                return View(order);
            }
        }

        public ActionResult Complete(int id)
        {
            return View(id);
        }
    }
}

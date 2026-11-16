using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebInfrastructure.Models;
using WebInfrastructure.Services;

namespace WebInfrastructure.Controllers;

/// <summary>Checkout: the address and payment form, order processing and the confirmation page.</summary>
public class CheckoutController : Controller
{
    private readonly IOrderProcessingService _orders;
    private readonly ICartIdProvider _cartIds;

    public CheckoutController(IOrderProcessingService orders, ICartIdProvider cartIds)
    {
        _orders = orders;
        _cartIds = cartIds;
    }

    public IActionResult Index()
    {
        return RedirectToAction(nameof(AddressAndPayment));
    }

    [HttpGet]
    public IActionResult AddressAndPayment()
    {
        return View(new Order());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddressAndPayment(
        [Bind("FirstName,LastName,Address,City,State,PostalCode,Country,Phone,Email")] Order order)
    {
        if (!ModelState.IsValid)
        {
            return View(order);
        }

        order.Username = "Anonymous";
        order.OrderDate = DateTime.UtcNow;

        try
        {
            var orderId = _orders.ProcessOrder(order, _cartIds.GetCartId(HttpContext));
            return RedirectToAction(nameof(Complete), new { id = orderId });
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Your order could not be saved. Please try again.");
            return View(order);
        }
    }

    public IActionResult Complete(int? id)
    {
        if (id is null)
        {
            // No usable order number in the address: there is no order to confirm.
            return NotFound();
        }

        return View(id.Value);
    }
}

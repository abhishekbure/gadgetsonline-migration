using Microsoft.AspNetCore.Mvc;
using WebInfrastructure.Models;
using WebInfrastructure.Services;

namespace WebInfrastructure.Controllers;

/// <summary>The session's shopping cart: show it, add a product, remove a product.</summary>
public class ShoppingCartController : Controller
{
    private readonly IShoppingCartService _cart;
    private readonly ICartIdProvider _cartIds;
    private readonly ILogger<ShoppingCartController> _logger;

    public ShoppingCartController(IShoppingCartService cart, ICartIdProvider cartIds, ILogger<ShoppingCartController> logger)
    {
        _cart = cart;
        _cartIds = cartIds;
        _logger = logger;
    }

    public IActionResult Index()
    {
        var items = _cart.GetCartItems(_cartIds.GetCartId(HttpContext));
        var viewModel = new ShoppingCartViewModel
        {
            CartItems = items,
            CartTotal = items.Sum(item => item.Count * (item.Product?.Price ?? 0M)),
        };
        return View(viewModel);
    }

    [AcceptVerbs("GET", "POST")]
    public IActionResult AddToCart(int id)
    {
        if (!_cart.AddToCart(_cartIds.GetCartId(HttpContext), id))
        {
            _logger.LogWarning("web_infrastructure.access.invalid product {ProductId} (add to cart)", id);
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }

    [AcceptVerbs("GET", "POST")]
    public IActionResult RemoveFromCart(int id)
    {
        if (_cart.RemoveFromCart(_cartIds.GetCartId(HttpContext), id) is null)
        {
            _logger.LogWarning("web_infrastructure.access.invalid product {ProductId} (remove from cart)", id);
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }
}

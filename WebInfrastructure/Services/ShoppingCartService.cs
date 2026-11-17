using Microsoft.EntityFrameworkCore;
using WebInfrastructure.Data;
using WebInfrastructure.Models;

namespace WebInfrastructure.Services;

/// <inheritdoc />
public class ShoppingCartService : IShoppingCartService
{
    private readonly GadgetsOnlineDbContext _db;
    private readonly ILogger<ShoppingCartService> _logger;

    public ShoppingCartService(GadgetsOnlineDbContext db, ILogger<ShoppingCartService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <inheritdoc />
    public List<Cart> GetCartItems(string cartId)
    {
        return _db.Carts
            .Include(c => c.Product)
            .Where(c => c.CartId == cartId)
            .OrderBy(c => c.RecordId)
            .ToList();
    }

    /// <inheritdoc />
    public decimal GetTotal(string cartId)
    {
        // Summed in the application, so the result does not depend on a database provider's decimal aggregates.
        return GetCartItems(cartId).Sum(item => item.Count * item.Product!.Price);
    }

    /// <inheritdoc />
    public int GetCount(string cartId)
    {
        return _db.Carts
            .Where(c => c.CartId == cartId)
            .Select(c => c.Count)
            .AsEnumerable()
            .Sum();
    }

    /// <inheritdoc />
    public bool AddToCart(string cartId, int productId)
    {
        if (!_db.Products.Any(p => p.ProductId == productId))
        {
            return false;
        }

        for (var attempt = 0; attempt < 2; attempt++)
        {
            var cartItem = FindLine(cartId, productId);
            if (cartItem is null)
            {
                _db.Carts.Add(new Cart
                {
                    ProductId = productId,
                    CartId = cartId,
                    Count = 1,
                    DateCreated = DateTime.UtcNow,
                });
            }
            else
            {
                cartItem.Count++;
            }

            try
            {
                _db.SaveChanges();
                break;
            }
            catch (DbUpdateException) when (attempt == 0)
            {
                // The same line was added by another request at the same moment: read it again and add to that one.
                _db.ChangeTracker.Clear();
            }
        }

        _logger.LogInformation("web_infrastructure.cart.modified {Action} product {ProductId}", "add", productId);
        return true;
    }

    /// <inheritdoc />
    public int? RemoveFromCart(string cartId, int productId)
    {
        var cartItem = FindLine(cartId, productId);
        if (cartItem is null)
        {
            return null;
        }

        int itemCount = 0;
        if (cartItem.Count > 1)
        {
            cartItem.Count--;
            itemCount = cartItem.Count;
        }
        else
        {
            _db.Carts.Remove(cartItem);
        }

        _db.SaveChanges();
        _logger.LogInformation("web_infrastructure.cart.modified {Action} product {ProductId}", "remove", productId);
        return itemCount;
    }

    /// <summary>The cart's line for the product; the oldest one if a double click ever left two.</summary>
    private Cart? FindLine(string cartId, int productId)
    {
        return _db.Carts
            .Where(c => c.CartId == cartId && c.ProductId == productId)
            .OrderBy(c => c.RecordId)
            .FirstOrDefault();
    }
}

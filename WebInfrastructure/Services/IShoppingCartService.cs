using WebInfrastructure.Models;

namespace WebInfrastructure.Services;

/// <summary>Reads and changes one session's shopping cart.</summary>
public interface IShoppingCartService
{
    List<Cart> GetCartItems(string cartId);

    decimal GetTotal(string cartId);

    int GetCount(string cartId);

    /// <summary>Adds one of the product to the cart; false when there is no such product.</summary>
    bool AddToCart(string cartId, int productId);

    /// <summary>Takes one of the product out of the cart; the count left, or null when the cart has no such line.</summary>
    int? RemoveFromCart(string cartId, int productId);
}

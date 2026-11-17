namespace WebInfrastructure.Models;

/// <summary>What the cart page shows: the session's cart lines and the cart total.</summary>
public class ShoppingCartViewModel
{
    public List<Cart> CartItems { get; set; } = new();

    public decimal CartTotal { get; set; }
}

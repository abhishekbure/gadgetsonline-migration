using WebInfrastructure.Models;

namespace WebInfrastructure.Services;

/// <summary>Turns a session's cart into a placed order.</summary>
public interface IOrderProcessingService
{
    /// <summary>Saves the order with one detail line per cart line and its total, empties the cart, and returns the order id.</summary>
    int ProcessOrder(Order order, string cartId);
}

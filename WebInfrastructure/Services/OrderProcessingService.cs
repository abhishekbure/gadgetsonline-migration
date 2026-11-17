using Microsoft.EntityFrameworkCore;
using WebInfrastructure.Data;
using WebInfrastructure.Models;

namespace WebInfrastructure.Services;

/// <inheritdoc />
public class OrderProcessingService : IOrderProcessingService
{
    private readonly GadgetsOnlineDbContext _db;
    private readonly ILogger<OrderProcessingService> _logger;

    public OrderProcessingService(GadgetsOnlineDbContext db, ILogger<OrderProcessingService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public int ProcessOrder(Order order, string cartId)
    {
        using var transaction = _db.Database.BeginTransaction();

        try
        {
            var cartItems = _db.Carts
                .Include(c => c.Product)
                .Where(c => c.CartId == cartId)
                .ToList();

            decimal orderTotal = 0;
            order.OrderDetails = new List<OrderDetail>();
            foreach (var item in cartItems)
            {
                // Product is included via EF Core Include but may be null if foreign key is broken.
                if (item.Product is null)
                {
                    throw new InvalidOperationException($"Cart item {item.RecordId} has no associated product.");
                }

                order.OrderDetails.Add(new OrderDetail
                {
                    ProductId = item.ProductId,
                    Quantity = item.Count,
                    UnitPrice = item.Product.Price,
                });
                orderTotal += item.Count * item.Product.Price;
            }

            order.Total = orderTotal;
            _db.Orders.Add(order);
            _db.Carts.RemoveRange(cartItems);
            _db.SaveChanges();
            transaction.Commit();
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }

        _logger.LogInformation("web_infrastructure.order.created order {OrderId} total {Total}", order.OrderId, order.Total);
        return order.OrderId;
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using WebInfrastructure.Models;
using WebInfrastructure.Services;

namespace WebInfrastructure.Tests;

/// <summary>Tests of turning a session's cart into an order.</summary>
public class OrderProcessingServiceTests
{
    private static Order NewOrder() => new()
    {
        FirstName = "Ada",
        LastName = "Lovelace",
        Address = "1 Analytical Way",
        City = "London",
        State = "LDN",
        PostalCode = "N1 9GU",
        Country = "UK",
        Phone = "555-0100",
        Email = "ada@example.com",
        Username = "Anonymous",
        OrderDate = new DateTime(2026, 11, 16),
    };

    [Fact]
    public void ProcessOrderSavesOneDetailPerCartLineWithTheTotalAndEmptiesTheCart()
    {
        using var db = new TestDatabase();
        var cart = new ShoppingCartService(db.NewContext(), NullLogger<ShoppingCartService>.Instance);
        cart.AddToCart("cart-a", 1);
        cart.AddToCart("cart-a", 1);
        cart.AddToCart("cart-a", 11);
        var orders = new OrderProcessingService(db.NewContext(), NullLogger<OrderProcessingService>.Instance);

        var orderId = orders.ProcessOrder(NewOrder(), "cart-a");

        Assert.True(orderId > 0);
        using var check = db.NewContext();
        var saved = check.Orders.Include(o => o.OrderDetails).Single(o => o.OrderId == orderId);
        Assert.Equal("Ada", saved.FirstName);
        Assert.Equal(699.00M * 2 + 9.99M, saved.Total);
        Assert.Equal(2, saved.OrderDetails.Count);
        var phone = saved.OrderDetails.Single(d => d.ProductId == 1);
        Assert.Equal(2, phone.Quantity);
        Assert.Equal(699.00M, phone.UnitPrice);
        Assert.Empty(check.Carts.Where(c => c.CartId == "cart-a"));
    }

    [Fact]
    public void ProcessOrderWithAnEmptyCartSavesAnOrderWithTotalZero()
    {
        using var db = new TestDatabase();
        var orders = new OrderProcessingService(db.NewContext(), NullLogger<OrderProcessingService>.Instance);

        var orderId = orders.ProcessOrder(NewOrder(), "cart-empty");

        using var check = db.NewContext();
        var saved = check.Orders.Include(o => o.OrderDetails).Single(o => o.OrderId == orderId);
        Assert.Equal(0M, saved.Total);
        Assert.Empty(saved.OrderDetails);
    }

    [Fact]
    public void ProcessOrderTakesOnlyTheGivenCartsLines()
    {
        using var db = new TestDatabase();
        var cart = new ShoppingCartService(db.NewContext(), NullLogger<ShoppingCartService>.Instance);
        cart.AddToCart("cart-a", 1);
        cart.AddToCart("cart-b", 2);
        var orders = new OrderProcessingService(db.NewContext(), NullLogger<OrderProcessingService>.Instance);

        var orderId = orders.ProcessOrder(NewOrder(), "cart-a");

        using var check = db.NewContext();
        var saved = check.Orders.Include(o => o.OrderDetails).Single(o => o.OrderId == orderId);
        Assert.Equal(699.00M, saved.Total);
        Assert.Equal(1, Assert.Single(saved.OrderDetails).ProductId);
        Assert.Equal(2, Assert.Single(check.Carts.Where(c => c.CartId == "cart-b")).ProductId);
    }
}

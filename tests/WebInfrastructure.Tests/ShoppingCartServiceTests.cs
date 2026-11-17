using Microsoft.Extensions.Logging.Abstractions;
using WebInfrastructure.Services;

namespace WebInfrastructure.Tests;

/// <summary>Tests of the session cart: adding, removing, counting and totalling.</summary>
public class ShoppingCartServiceTests
{
    private static ShoppingCartService NewService(TestDatabase db) =>
        new(db.NewContext(), NullLogger<ShoppingCartService>.Instance);

    [Fact]
    public void AddToCartCreatesALineWithCountOne()
    {
        using var db = new TestDatabase();
        var cart = NewService(db);

        Assert.True(cart.AddToCart("cart-a", 1));

        var items = cart.GetCartItems("cart-a");
        var line = Assert.Single(items);
        Assert.Equal(1, line.ProductId);
        Assert.Equal(1, line.Count);
        Assert.Equal("Phone 12", line.Product?.Name);
    }

    [Fact]
    public void AddingTheSameProductAgainRaisesTheCount()
    {
        using var db = new TestDatabase();
        var cart = NewService(db);

        cart.AddToCart("cart-a", 1);
        cart.AddToCart("cart-a", 1);

        var line = Assert.Single(cart.GetCartItems("cart-a"));
        Assert.Equal(2, line.Count);
        Assert.Equal(1398.00M, cart.GetTotal("cart-a"));
    }

    [Fact]
    public void AddToCartRefusesAnUnknownProduct()
    {
        using var db = new TestDatabase();
        var cart = NewService(db);

        Assert.False(cart.AddToCart("cart-a", 99999));
        Assert.Empty(cart.GetCartItems("cart-a"));
    }

    [Fact]
    public void CartsOfDifferentSessionsAreKeptApart()
    {
        using var db = new TestDatabase();
        var cart = NewService(db);

        cart.AddToCart("cart-a", 1);
        cart.AddToCart("cart-b", 2);

        Assert.Equal(1, Assert.Single(cart.GetCartItems("cart-a")).ProductId);
        Assert.Equal(2, Assert.Single(cart.GetCartItems("cart-b")).ProductId);
        Assert.Equal(699.00M, cart.GetTotal("cart-a"));
        Assert.Equal(999.00M, cart.GetTotal("cart-b"));
    }

    [Fact]
    public void TotalAndCountCoverEveryLine()
    {
        using var db = new TestDatabase();
        var cart = NewService(db);

        cart.AddToCart("cart-a", 1);
        cart.AddToCart("cart-a", 1);
        cart.AddToCart("cart-a", 11);

        Assert.Equal(3, cart.GetCount("cart-a"));
        Assert.Equal(699.00M * 2 + 9.99M, cart.GetTotal("cart-a"));
    }

    [Fact]
    public void AnEmptyCartHasNoItemsNoCountAndTotalZero()
    {
        using var db = new TestDatabase();
        var cart = NewService(db);

        Assert.Empty(cart.GetCartItems("cart-a"));
        Assert.Equal(0, cart.GetCount("cart-a"));
        Assert.Equal(0M, cart.GetTotal("cart-a"));
    }

    [Fact]
    public void RemoveFromCartLowersTheCountThenDropsTheLine()
    {
        using var db = new TestDatabase();
        var cart = NewService(db);
        cart.AddToCart("cart-a", 1);
        cart.AddToCart("cart-a", 1);

        Assert.Equal(1, cart.RemoveFromCart("cart-a", 1));
        Assert.Equal(1, Assert.Single(cart.GetCartItems("cart-a")).Count);

        Assert.Equal(0, cart.RemoveFromCart("cart-a", 1));
        Assert.Empty(cart.GetCartItems("cart-a"));
    }

    [Fact]
    public void ADoubleClickThatLeftTwoLinesDoesNotBreakTheCart()
    {
        using var db = new TestDatabase();
        using (var setup = db.NewContext())
        {
            setup.Carts.Add(new WebInfrastructure.Models.Cart { CartId = "cart-a", ProductId = 1, Count = 1, DateCreated = DateTime.Now });
            setup.Carts.Add(new WebInfrastructure.Models.Cart { CartId = "cart-a", ProductId = 1, Count = 1, DateCreated = DateTime.Now });
            setup.SaveChanges();
        }

        var cart = NewService(db);

        Assert.True(cart.AddToCart("cart-a", 1));
        Assert.Equal(2, cart.GetCartItems("cart-a").First().Count);
        Assert.Equal(1, cart.RemoveFromCart("cart-a", 1));
    }

    [Fact]
    public void RemoveFromCartReportsAProductThatIsNotInTheCart()
    {
        using var db = new TestDatabase();
        var cart = NewService(db);
        cart.AddToCart("cart-a", 1);

        Assert.Null(cart.RemoveFromCart("cart-a", 2));
        Assert.Null(cart.RemoveFromCart("cart-b", 1));
        Assert.Single(cart.GetCartItems("cart-a"));
    }
}

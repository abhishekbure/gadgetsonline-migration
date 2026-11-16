using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using WebInfrastructure.Controllers;
using WebInfrastructure.Models;
using WebInfrastructure.Services;

namespace WebInfrastructure.Tests;

/// <summary>Tests of the cart page and of adding and removing products.</summary>
public class ShoppingCartControllerTests
{
    private static ShoppingCartController NewController(TestDatabase db, Microsoft.AspNetCore.Http.HttpContext context)
    {
        var cart = new ShoppingCartService(db.NewContext(), NullLogger<ShoppingCartService>.Instance);
        return new ShoppingCartController(cart, new SessionCartProvider(), NullLogger<ShoppingCartController>.Instance)
            .WithContext(context);
    }

    [Fact]
    public void IndexShowsAnEmptyCartForANewSession()
    {
        using var db = new TestDatabase();
        var controller = NewController(db, TestHttp.NewContext());

        var result = Assert.IsType<ViewResult>(controller.Index());

        var model = Assert.IsType<ShoppingCartViewModel>(result.Model);
        Assert.Empty(model.CartItems);
        Assert.Equal(0M, model.CartTotal);
    }

    [Fact]
    public void AddToCartRedirectsToTheCartAndTheProductIsThereAfterwards()
    {
        using var db = new TestDatabase();
        var controller = NewController(db, TestHttp.NewContext());

        var redirect = Assert.IsType<RedirectToActionResult>(controller.AddToCart(1));

        Assert.Equal("Index", redirect.ActionName);
        var model = Assert.IsType<ShoppingCartViewModel>(Assert.IsType<ViewResult>(controller.Index()).Model);
        var line = Assert.Single(model.CartItems);
        Assert.Equal("Phone 12", line.Product?.Name);
        Assert.Equal(699.00M, model.CartTotal);
    }

    [Fact]
    public void AddingTwiceAddsToTheSameLineOfTheSameSession()
    {
        using var db = new TestDatabase();
        var controller = NewController(db, TestHttp.NewContext());

        controller.AddToCart(1);
        controller.AddToCart(1);

        var model = Assert.IsType<ShoppingCartViewModel>(Assert.IsType<ViewResult>(controller.Index()).Model);
        Assert.Equal(2, Assert.Single(model.CartItems).Count);
        Assert.Equal(1398.00M, model.CartTotal);
    }

    [Fact]
    public void ADifferentSessionDoesNotSeeTheCart()
    {
        using var db = new TestDatabase();
        NewController(db, TestHttp.NewContext()).AddToCart(1);
        var other = NewController(db, TestHttp.NewContext());

        var model = Assert.IsType<ShoppingCartViewModel>(Assert.IsType<ViewResult>(other.Index()).Model);

        Assert.Empty(model.CartItems);
    }

    [Fact]
    public void AddingAnUnknownProductIsNotFound()
    {
        using var db = new TestDatabase();
        var controller = NewController(db, TestHttp.NewContext());

        Assert.IsType<NotFoundResult>(controller.AddToCart(99999));
    }

    [Fact]
    public void RemoveFromCartRedirectsToTheCartAndLowersTheCount()
    {
        using var db = new TestDatabase();
        var controller = NewController(db, TestHttp.NewContext());
        controller.AddToCart(1);
        controller.AddToCart(1);

        var redirect = Assert.IsType<RedirectToActionResult>(controller.RemoveFromCart(1));

        Assert.Equal("Index", redirect.ActionName);
        var model = Assert.IsType<ShoppingCartViewModel>(Assert.IsType<ViewResult>(controller.Index()).Model);
        Assert.Equal(1, Assert.Single(model.CartItems).Count);
    }

    [Fact]
    public void RemovingAProductThatIsNotInTheCartIsNotFound()
    {
        using var db = new TestDatabase();
        var controller = NewController(db, TestHttp.NewContext());

        Assert.IsType<NotFoundResult>(controller.RemoveFromCart(1));
    }
}

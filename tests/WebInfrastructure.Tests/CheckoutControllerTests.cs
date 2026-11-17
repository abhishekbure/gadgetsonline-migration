using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using WebInfrastructure.Controllers;
using WebInfrastructure.Models;
using WebInfrastructure.Services;

namespace WebInfrastructure.Tests;

/// <summary>Tests of the checkout: the form, placing the order and the confirmation page.</summary>
public class CheckoutControllerTests
{
    private static CheckoutController NewController(TestDatabase db, Microsoft.AspNetCore.Http.HttpContext context)
    {
        var orders = new OrderProcessingService(db.NewContext(), NullLogger<OrderProcessingService>.Instance);
        return new CheckoutController(orders, new SessionCartProvider()).WithContext(context);
    }

    private static Order FilledForm() => new()
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
    };

    [Fact]
    public void IndexLeadsToTheAddressAndPaymentForm()
    {
        using var db = new TestDatabase();
        var controller = NewController(db, TestHttp.NewContext());

        var redirect = Assert.IsType<RedirectToActionResult>(controller.Index());

        Assert.Equal("AddressAndPayment", redirect.ActionName);
    }

    [Fact]
    public void TheFormStartsWithAnEmptyOrder()
    {
        using var db = new TestDatabase();
        var controller = NewController(db, TestHttp.NewContext());

        var result = Assert.IsType<ViewResult>(controller.AddressAndPayment());

        Assert.IsType<Order>(result.Model);
    }

    [Fact]
    public void AnInvalidFormIsShownAgainAndNothingIsSaved()
    {
        using var db = new TestDatabase();
        var controller = NewController(db, TestHttp.NewContext());
        var form = FilledForm();
        controller.ModelState.AddModelError("Email", "Email is is not valid.");

        var result = Assert.IsType<ViewResult>(controller.AddressAndPayment(form));

        Assert.Same(form, result.Model);
        Assert.Empty(db.NewContext().Orders);
    }

    [Fact]
    public void AValidFormPlacesTheOrderFromTheCartAndRedirectsToTheConfirmation()
    {
        using var db = new TestDatabase();
        var context = TestHttp.NewContext();
        var cart = new ShoppingCartController(
            new ShoppingCartService(db.NewContext(), NullLogger<ShoppingCartService>.Instance),
            new SessionCartProvider(),
            NullLogger<ShoppingCartController>.Instance).WithContext(context);
        cart.AddToCart(1);
        cart.AddToCart(11);
        var controller = NewController(db, context);

        var redirect = Assert.IsType<RedirectToActionResult>(controller.AddressAndPayment(FilledForm()));

        Assert.Equal("Complete", redirect.ActionName);
        var orderId = Assert.IsType<int>(redirect.RouteValues!["id"]);
        using var check = db.NewContext();
        var saved = check.Orders.Include(o => o.OrderDetails).Single(o => o.OrderId == orderId);
        Assert.Equal("Anonymous", saved.Username);
        Assert.Equal(699.00M + 9.99M, saved.Total);
        Assert.Equal(2, saved.OrderDetails.Count);
        Assert.Empty(check.Carts);
    }

    [Fact]
    public void CompleteShowsTheOrderNumber()
    {
        using var db = new TestDatabase();
        var controller = NewController(db, TestHttp.NewContext());

        var result = Assert.IsType<ViewResult>(controller.Complete(42));

        Assert.Equal(42, result.Model);
    }

    [Fact]
    public void CompleteWithoutAnOrderNumberIsNotFound()
    {
        using var db = new TestDatabase();
        var controller = NewController(db, TestHttp.NewContext());

        Assert.IsType<NotFoundResult>(controller.Complete(null));
    }

    [Fact]
    public void ThePostActionRequiresAnAntiForgeryToken()
    {
        var post = typeof(CheckoutController).GetMethods()
            .Single(m => m.Name == nameof(CheckoutController.AddressAndPayment)
                         && m.GetCustomAttributes(typeof(HttpPostAttribute), false).Length == 1);

        Assert.NotEmpty(post.GetCustomAttributes(typeof(ValidateAntiForgeryTokenAttribute), false));
    }

    [Fact]
    public void ThePostActionBindsOnlyTheShippingFields()
    {
        var post = typeof(CheckoutController).GetMethods()
            .Single(m => m.Name == nameof(CheckoutController.AddressAndPayment)
                         && m.GetCustomAttributes(typeof(HttpPostAttribute), false).Length == 1);
        var bind = (BindAttribute)post.GetParameters().Single().GetCustomAttributes(typeof(BindAttribute), false).Single();

        Assert.Equal(
            new[] { "Address", "City", "Country", "Email", "FirstName", "LastName", "Phone", "PostalCode", "State" },
            bind.Include.OrderBy(name => name, StringComparer.Ordinal));
        Assert.DoesNotContain("Total", bind.Include);
        Assert.DoesNotContain("Username", bind.Include);
        Assert.DoesNotContain("OrderId", bind.Include);
        Assert.DoesNotContain("OrderDate", bind.Include);
    }
}

using Microsoft.AspNetCore.Mvc;
using WebInfrastructure.Controllers;
using WebInfrastructure.Models;
using WebInfrastructure.Services;

namespace WebInfrastructure.Tests;

/// <summary>Tests of the home page and the informational pages.</summary>
public class HomeControllerTests
{
    [Fact]
    public void IndexShowsTheSixBestSellers()
    {
        using var db = new TestDatabase();
        var controller = new HomeController(new InventoryService(db.NewContext()));

        var result = Assert.IsType<ViewResult>(controller.Index());

        var products = Assert.IsAssignableFrom<List<Product>>(result.Model);
        Assert.Equal(6, products.Count);
        Assert.Equal("Phone 12", products[0].Name);
    }

    [Fact]
    public void AboutCarriesItsMessage()
    {
        using var db = new TestDatabase();
        var controller = new HomeController(new InventoryService(db.NewContext()));

        var result = Assert.IsType<ViewResult>(controller.About());

        Assert.Equal("Your application description page.", result.ViewData["Message"]);
    }

    [Fact]
    public void ContactCarriesItsMessage()
    {
        using var db = new TestDatabase();
        var controller = new HomeController(new InventoryService(db.NewContext()));

        var result = Assert.IsType<ViewResult>(controller.Contact());

        Assert.Equal("Your contact page.", result.ViewData["Message"]);
    }

    [Fact]
    public void ErrorShowsTheErrorViewWithStatus500()
    {
        using var db = new TestDatabase();
        var context = TestHttp.NewContext();
        var controller = new HomeController(new InventoryService(db.NewContext())).WithContext(context);

        Assert.IsType<ViewResult>(controller.Error());

        Assert.Equal(500, context.Response.StatusCode);
    }
}

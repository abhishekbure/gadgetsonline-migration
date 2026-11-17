using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using WebInfrastructure.Controllers;
using WebInfrastructure.Models;
using WebInfrastructure.Services;

namespace WebInfrastructure.Tests;

/// <summary>Tests of browsing the catalogue.</summary>
public class StoreControllerTests
{
    private static StoreController NewController(TestDatabase db) =>
        new(new InventoryService(db.NewContext()), NullLogger<StoreController>.Instance);

    [Fact]
    public void IndexListsEveryCategory()
    {
        using var db = new TestDatabase();

        var result = Assert.IsType<ViewResult>(NewController(db).Index());

        Assert.Equal(5, Assert.IsAssignableFrom<List<Category>>(result.Model).Count);
    }

    [Fact]
    public void BrowseListsTheProductsOfTheCategory()
    {
        using var db = new TestDatabase();

        var result = Assert.IsType<ViewResult>(NewController(db).Browse("Laptops"));

        var products = Assert.IsAssignableFrom<List<Product>>(result.Model);
        Assert.Equal(new[] { "XTS 13'", "PC 15.5'", "Notebook 14" }, products.Select(p => p.Name));
    }

    [Fact]
    public void BrowseShowsAnEmptyListForAnUnknownCategory()
    {
        using var db = new TestDatabase();

        var result = Assert.IsType<ViewResult>(NewController(db).Browse("NoSuchCategory"));

        Assert.Empty(Assert.IsAssignableFrom<List<Product>>(result.Model));
    }

    [Fact]
    public void BrowseWithoutACategoryShowsAnEmptyList()
    {
        using var db = new TestDatabase();

        var result = Assert.IsType<ViewResult>(NewController(db).Browse(null));

        Assert.Empty(Assert.IsAssignableFrom<List<Product>>(result.Model));
    }

    [Fact]
    public void DetailsShowsTheProduct()
    {
        using var db = new TestDatabase();

        var result = Assert.IsType<ViewResult>(NewController(db).Details(5));

        var product = Assert.IsType<Product>(result.Model);
        Assert.Equal("PC 15.5'", product.Name);
        Assert.Equal("Laptops", product.Category?.Name);
    }

    [Fact]
    public void DetailsOfAnUnknownProductIsNotFound()
    {
        using var db = new TestDatabase();

        Assert.IsType<NotFoundResult>(NewController(db).Details(99999));
    }
}

using WebInfrastructure.Services;

namespace WebInfrastructure.Tests;

/// <summary>Tests of the catalogue reads against the seeded sample data.</summary>
public class InventoryServiceTests
{
    [Fact]
    public void SeedCreatesFiveCategoriesAndThirteenProducts()
    {
        using var db = new TestDatabase();
        using var context = db.NewContext();

        Assert.Equal(5, context.Categories.Count());
        Assert.Equal(13, context.Products.Count());
    }

    [Fact]
    public void SeedingTwiceDoesNotDuplicateTheCatalogue()
    {
        using var db = new TestDatabase();
        using var context = db.NewContext();

        WebInfrastructure.Data.GadgetsOnlineDbInitializer.Seed(context);

        Assert.Equal(13, context.Products.Count());
    }

    [Fact]
    public void GetBestSellersReturnsTheFirstProductsInCatalogueOrder()
    {
        using var db = new TestDatabase();
        var inventory = new InventoryService(db.NewContext());

        var names = inventory.GetBestSellers(6).Select(p => p.Name).ToList();

        Assert.Equal(
            new[] { "Phone 12", "Phone 13 Pro", "Phone 13 Pro Max", "XTS 13'", "PC 15.5'", "Notebook 14" },
            names);
    }

    [Fact]
    public void GetAllCategoriesReturnsEveryCategoryInOrder()
    {
        using var db = new TestDatabase();
        var inventory = new InventoryService(db.NewContext());

        var names = inventory.GetAllCategories().Select(c => c.Name).ToList();

        Assert.Equal(new[] { "Mobile Phones", "Laptops", "Desktops", "Audio", "Accessories" }, names);
    }

    [Theory]
    [InlineData("Mobile Phones", 3)]
    [InlineData("Laptops", 3)]
    [InlineData("Desktops", 2)]
    [InlineData("Audio", 2)]
    [InlineData("Accessories", 3)]
    public void GetAllProductsInCategoryReturnsOnlyThatCategoriesProducts(string category, int expected)
    {
        using var db = new TestDatabase();
        var inventory = new InventoryService(db.NewContext());

        var products = inventory.GetAllProductsInCategory(category);

        Assert.Equal(expected, products.Count);
        Assert.All(products, p => Assert.Equal(category, db.NewContext().Categories.Single(c => c.CategoryId == p.CategoryId).Name));
    }

    [Theory]
    [InlineData("laptops")]
    [InlineData("LAPTOPS")]
    [InlineData("LaPtOpS")]
    public void CategoryNamesMatchWithoutRegardToCase(string category)
    {
        using var db = new TestDatabase();
        var inventory = new InventoryService(db.NewContext());

        var names = inventory.GetAllProductsInCategory(category).Select(p => p.Name).ToList();

        Assert.Equal(new[] { "XTS 13'", "PC 15.5'", "Notebook 14" }, names);
    }

    [Theory]
    [InlineData("Laptops ")]
    [InlineData("laptops   ")]
    public void TrailingBlanksInTheCategoryNameAreIgnored(string category)
    {
        using var db = new TestDatabase();
        var inventory = new InventoryService(db.NewContext());

        Assert.Equal(3, inventory.GetAllProductsInCategory(category).Count);
    }

    [Fact]
    public void LeadingBlanksInTheCategoryNameAreNotIgnored()
    {
        using var db = new TestDatabase();
        var inventory = new InventoryService(db.NewContext());

        Assert.Empty(inventory.GetAllProductsInCategory(" Laptops"));
    }

    [Theory]
    [InlineData("NoSuchCategory")]
    [InlineData("")]
    [InlineData(null)]
    public void GetAllProductsInCategoryReturnsNothingForAnUnknownCategory(string? category)
    {
        using var db = new TestDatabase();
        var inventory = new InventoryService(db.NewContext());

        Assert.Empty(inventory.GetAllProductsInCategory(category));
    }

    [Fact]
    public void GetProductByIdReturnsTheProductWithItsCategory()
    {
        using var db = new TestDatabase();
        var inventory = new InventoryService(db.NewContext());

        var product = inventory.GetProductById(1);

        Assert.NotNull(product);
        Assert.Equal("Phone 12", product.Name);
        Assert.Equal(699.00M, product.Price);
        Assert.Equal("Mobile Phones", product.Category?.Name);
    }

    [Fact]
    public void GetProductByIdReturnsNullForAnUnknownProduct()
    {
        using var db = new TestDatabase();
        var inventory = new InventoryService(db.NewContext());

        Assert.Null(inventory.GetProductById(99999));
    }
}

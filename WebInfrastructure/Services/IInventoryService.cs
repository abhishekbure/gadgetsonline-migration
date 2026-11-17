using WebInfrastructure.Models;

namespace WebInfrastructure.Services;

/// <summary>Reads the product catalogue: best sellers, categories, products by category or id.</summary>
public interface IInventoryService
{
    List<Product> GetBestSellers(int count);

    List<Category> GetAllCategories();

    List<Product> GetAllProductsInCategory(string? category);

    Product? GetProductById(int id);
}

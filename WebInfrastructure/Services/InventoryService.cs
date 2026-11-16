using Microsoft.EntityFrameworkCore;
using WebInfrastructure.Data;
using WebInfrastructure.Models;

namespace WebInfrastructure.Services;

/// <inheritdoc />
public class InventoryService : IInventoryService
{
    private readonly GadgetsOnlineDbContext _db;

    public InventoryService(GadgetsOnlineDbContext db)
    {
        _db = db;
    }

    public List<Product> GetBestSellers(int count)
    {
        return _db.Products
            .OrderBy(p => p.ProductId)
            .Take(count)
            .ToList();
    }

    public List<Category> GetAllCategories()
    {
        return _db.Categories
            .OrderBy(c => c.CategoryId)
            .ToList();
    }

    public List<Product> GetAllProductsInCategory(string? category)
    {
        if (category is null)
        {
            return new List<Product>();
        }

        // Category names match without regard to case and trailing blanks, as SQL Server does by default,
        // so the answer does not depend on the database provider.
        var wanted = category.TrimEnd().ToLower();
        return _db.Products
            .Where(p => p.Category!.Name.ToLower() == wanted)
            .OrderBy(p => p.ProductId)
            .ToList();
    }

    public Product? GetProductById(int id)
    {
        return _db.Products
            .Include(p => p.Category)
            .FirstOrDefault(p => p.ProductId == id);
    }
}

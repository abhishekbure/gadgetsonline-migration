using System.Collections.Generic;
using System.Linq;
using GadgetsOnline.Models;
using Microsoft.EntityFrameworkCore;

namespace GadgetsOnline.Services
{
    public class Inventory
    {
        private readonly GadgetsOnlineEntities _store;

        public Inventory(GadgetsOnlineEntities store) { _store = store; }

        public List<Product> GetBestSellers(int count)
        {
            return _store.Products
                    .Include(p => p.Category)
                    .OrderBy(p => p.ProductId)
                    .Take(count)
                    .ToList();
        }

        public List<Category> GetAllCategories()
        {
            return _store.Categories.ToList();
        }

        public List<Product> GetAllProductsInCategory(string category)
        {
            return _store.Products
                    .Where(p => p.Category!.Name == category)
                    .ToList();
        }

        public Product? GetProductById(int id)
        {
            return _store.Products
                   .Include(p => p.Category)
                   .Where(p => p.ProductId == id)
                   .FirstOrDefault();
        }

        internal string GetProductNameById(int id)
        {
            var product = _store.Products
                   .Where(p => p.ProductId == id)
                   .FirstOrDefault();
            return product?.Name ?? "Unknown product";
        }
    }
}

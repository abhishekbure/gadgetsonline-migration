using WebInfrastructure.Models;

namespace WebInfrastructure.Data;

/// <summary>Creates the sample catalogue the store starts with: five categories and thirteen products.</summary>
public static class GadgetsOnlineDbInitializer
{
    public static void Seed(GadgetsOnlineDbContext context)
    {
        if (context.Categories.Any())
        {
            return;
        }

        var mobile = new Category { Name = "Mobile Phones", Description = "Latest collection of Mobile Phones" };
        var laptops = new Category { Name = "Laptops", Description = "Latest Laptops in 2022" };
        var desktops = new Category { Name = "Desktops", Description = "Latest Desktops in 2022" };
        var audio = new Category { Name = "Audio", Description = "Latest audio devices" };
        var accessories = new Category { Name = "Accessories", Description = "USB Cables, Mobile chargers and Keyboards etc" };
        context.Categories.AddRange(mobile, laptops, desktops, audio, accessories);

        context.Products.AddRange(
            new Product { Category = mobile, Name = "Phone 12", Price = 699.00M, ProductArtUrl = "/Content/Images/Mobile/1.jpg" },
            new Product { Category = mobile, Name = "Phone 13 Pro", Price = 999.00M, ProductArtUrl = "/Content/Images/Mobile/2.jpg" },
            new Product { Category = mobile, Name = "Phone 13 Pro Max", Price = 1199.00M, ProductArtUrl = "/Content/Images/Mobile/3.jpg" },
            new Product { Category = laptops, Name = "XTS 13'", Price = 899.00M, ProductArtUrl = "/Content/Images/Laptop/1.jpg" },
            new Product { Category = laptops, Name = "PC 15.5'", Price = 479.00M, ProductArtUrl = "/Content/Images/Laptop/2.jpg" },
            new Product { Category = laptops, Name = "Notebook 14", Price = 169.00M, ProductArtUrl = "/Content/Images/Laptop/3.jpg" },
            new Product { Category = desktops, Name = "The IdeaCenter", Price = 539.00M, ProductArtUrl = "/Content/Images/placeholder.gif" },
            new Product { Category = desktops, Name = "COMP 22-df003w", Price = 389.00M, ProductArtUrl = "/Content/Images/placeholder.gif" },
            new Product { Category = audio, Name = "Bluetooth Headphones Over Ear", Price = 28.00M, ProductArtUrl = "/Content/Images/Headphones/1.png" },
            new Product { Category = audio, Name = "ZX Series ", Price = 10.00M, ProductArtUrl = "/Content/Images/Headphones/2.png" },
            new Product { Category = accessories, Name = "Wireless charger", Price = 9.99M, ProductArtUrl = "/Content/Images/placeholder.gif" },
            new Product { Category = accessories, Name = "Mousepad", Price = 2.99M, ProductArtUrl = "/Content/Images/placeholder.gif" },
            new Product { Category = accessories, Name = "Keyboard", Price = 9.99M, ProductArtUrl = "/Content/Images/placeholder.gif" });

        context.SaveChanges();
    }
}

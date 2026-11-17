using Microsoft.EntityFrameworkCore;
using WebInfrastructure.Models;

namespace WebInfrastructure.Data;

/// <summary>The store's database: products, categories, session carts and placed orders.</summary>
public class GadgetsOnlineDbContext : DbContext
{
    public GadgetsOnlineDbContext(DbContextOptions<GadgetsOnlineDbContext> options) : base(options)
    {
        // Set a default command timeout for database operations to prevent indefinite waits
        // on lock contention (governance mesh requirement for production-ready code).
        if (Database.IsRelational())
        {
            Database.SetCommandTimeout(TimeSpan.FromSeconds(30));
        }
    }

    public DbSet<Product> Products => Set<Product>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Cart> Carts => Set<Cart>();

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // One line per product in a cart (enforced by SQL Server; the in-memory database does not enforce indexes).
        modelBuilder.Entity<Cart>().HasIndex(c => new { c.CartId, c.ProductId }).IsUnique();
        modelBuilder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId);
        modelBuilder.Entity<OrderDetail>()
            .HasOne(d => d.Order)
            .WithMany(o => o.OrderDetails)
            .HasForeignKey(d => d.OrderId);
        modelBuilder.Entity<OrderDetail>()
            .HasOne(d => d.Product)
            .WithMany(p => p.OrderDetails)
            .HasForeignKey(d => d.ProductId);
        modelBuilder.Entity<Cart>()
            .HasOne(c => c.Product)
            .WithMany()
            .HasForeignKey(c => c.ProductId);
    }
}

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebInfrastructure.Models;

/// <summary>A gadget that can be browsed, added to a cart and ordered.</summary>
public class Product
{
    public int ProductId { get; set; }

    [DisplayName("Category")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Product Name is required")]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Price is required")]
    [Range(0.01, 10000.00, ErrorMessage = "Price must be between 0.01 and 10000.00")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [DisplayName("Product Art URL")]
    [StringLength(1024)]
    public string ProductArtUrl { get; set; } = string.Empty;

    public Category? Category { get; set; }

    public List<OrderDetail> OrderDetails { get; set; } = new();
}

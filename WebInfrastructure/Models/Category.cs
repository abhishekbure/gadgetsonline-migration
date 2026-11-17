using System.ComponentModel.DataAnnotations;

namespace WebInfrastructure.Models;

/// <summary>A product category shown in the category menu and used to browse products.</summary>
public class Category
{
    public int CategoryId { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public List<Product> Products { get; set; } = new();
}

using System.ComponentModel.DataAnnotations;

namespace WebInfrastructure.Models;

/// <summary>One line of a session's shopping cart: a product and how many of it.</summary>
public class Cart
{
    [Key]
    public int RecordId { get; set; }

    public string CartId { get; set; } = string.Empty;

    public int ProductId { get; set; }

    public int Count { get; set; }

    public DateTime DateCreated { get; set; }

    public Product? Product { get; set; }
}

using System.ComponentModel.DataAnnotations.Schema;

namespace WebInfrastructure.Models;

/// <summary>One product line of a placed order, with the price at the time of ordering.</summary>
public class OrderDetail
{
    public int OrderDetailId { get; set; }

    public int OrderId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    public Product? Product { get; set; }

    public Order? Order { get; set; }
}

using System.Collections.Generic;

namespace GadgetsOnline.Models
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public List<Product> Products { get; set; } = new List<Product>();
    }
}

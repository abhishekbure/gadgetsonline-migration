
namespace GadgetsOnline.ViewModel
{
    public class ShoppingCartRemoveViewModel
    {
        public string Message { get; set; } = null!;
        public decimal CartTotal { get; set; }
        public int CartCount { get; set; }
        public int ItemCount { get; set; }
        public int DeleteId { get; set; }
    }
}

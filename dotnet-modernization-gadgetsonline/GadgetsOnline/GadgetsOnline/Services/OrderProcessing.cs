using GadgetsOnline.Models;

namespace GadgetsOnline.Services
{
    public class OrderProcessing
    {
        private readonly GadgetsOnlineEntities _store;
        private readonly ShoppingCart _cart;

        public OrderProcessing(GadgetsOnlineEntities store, ShoppingCart cart)
        {
            _store = store;
            _cart = cart;
        }

        internal bool ProcessOrder(Order order)
        {
            _store.Orders.Add(order);
            _store.SaveChanges();

            _cart.CreateOrder(order);

            return true;
        }
    }
}

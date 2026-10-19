using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using GadgetsOnline.Models;

namespace GadgetsOnline.Services
{
    public class ShoppingCart
    {
        private readonly GadgetsOnlineEntities db;
        private readonly IHttpContextAccessor _httpContextAccessor;
        string ShoppingCartId { get; set; }

        public const string CartSessionKey = "CartId";

        public ShoppingCart(GadgetsOnlineEntities db, IHttpContextAccessor httpContextAccessor)
        {
            this.db = db;
            _httpContextAccessor = httpContextAccessor;
            ShoppingCartId = GetCartId();
        }

        internal int CreateOrder(Order order)
        {
            decimal orderTotal = 0;

            var cartItems = GetCartItems();

            foreach (var item in cartItems)
            {
                var orderDetail = new OrderDetail
                {
                    ProductId = item.ProductId,
                    OrderId = order.OrderId,
                    UnitPrice = item.Product!.Price,
                    Quantity = item.Count
                };

                orderTotal += (item.Count * item.Product.Price);

                db.OrderDetails.Add(orderDetail);
            }

            order.Total = orderTotal;

            db.SaveChanges();

            EmptyCart();

            return order.OrderId;
        }

        private void EmptyCart()
        {
            var cartItems = db.Carts.Where(cart => cart.CartId == ShoppingCartId);

            foreach (var cartItem in cartItems)
            {
                db.Carts.Remove(cartItem);
            }

            db.SaveChanges();
        }

        public string GetCartId()
        {
            var session = _httpContextAccessor.HttpContext!.Session;
            if (session.GetString(CartSessionKey) == null)
            {
                var userName = _httpContextAccessor.HttpContext!.User.Identity?.Name;
                session.SetString(CartSessionKey, string.IsNullOrEmpty(userName) ? Guid.NewGuid().ToString() : userName);
            }
            return session.GetString(CartSessionKey)!;
        }

        public void AddToCart(int id)
        {
            var cartItem = db.Carts.SingleOrDefault(
                        c => c.CartId == ShoppingCartId
                        && c.ProductId == id);

            if (cartItem == null)
            {
                cartItem = new Cart
                {
                    ProductId = id,
                    CartId = ShoppingCartId,
                    Count = 1,
                    DateCreated = DateTime.Now
                };

                db.Carts.Add(cartItem);
            }
            else
            {
                cartItem.Count++;
            }

            db.SaveChanges();
        }

        public int GetCount()
        {
            int? count = (from cartItems in db.Carts
                          where cartItems.CartId == ShoppingCartId
                          select (int?)cartItems.Count).Sum();

            return count ?? 0;
        }

        internal int RemoveFromCart(int id)
        {
            var cartItem = db.Carts.SingleOrDefault(
                            cart => cart.CartId == ShoppingCartId
                            && cart.ProductId == id);

            if (cartItem == null) return 0;

            int itemCount = 0;

            if (cartItem != null)
            {
                if (cartItem.Count > 1)
                {
                    cartItem.Count--;
                    itemCount = cartItem.Count;
                }
                else
                {
                    db.Carts.Remove(cartItem);
                }

                db.SaveChanges();
            }

            return itemCount;
        }

        public List<Cart> GetCartItems()
        {
            return db.Carts
                .Include(c => c.Product)
                .Where(cart => cart.CartId == ShoppingCartId)
                .ToList();
        }

        public decimal GetTotal()
        {
            decimal? total = (from cartItems in db.Carts.Include(c => c.Product)
                              where cartItems.CartId == ShoppingCartId
                              select (int?)cartItems.Count * cartItems.Product!.Price).Sum();
            return total ?? decimal.Zero;
        }
    }
}

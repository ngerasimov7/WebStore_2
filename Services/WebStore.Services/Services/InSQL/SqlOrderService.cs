using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using WebStore.DAL.Context;
using WebStore.Domain.Entities.Identity;
using WebStore.Domain.Entities.Orders;
using WebStore.Interfaces.Servcies;
using WebStore.Domain.ViewModels;

namespace WebStore.Services.InSQL
{
    public class SqlOrderService : IOrderService
    {
        private readonly WebStoreDB _db;
        private readonly UserManager<User> _UserManager;

        public SqlOrderService(WebStoreDB db, UserManager<User> UserManager)
        {
            _db = db;
            _UserManager = UserManager;
        }

        public async Task<IEnumerable<Order>> GetUserOrders(string UserName) => await _db.Orders
           .Include(order => order.User)
           .Include(order => order.Items)
           .Where(order => order.User.UserName == UserName)
           .ToArrayAsync();

        public async Task<Order> GetOrderById(int id) => await _db.Orders
           .Include(order => order.User)
           .Include(order => order.Items)
           .FirstOrDefaultAsync(order => order.Id == id);

        public async Task<Order> CreateOrder(string UserName, CartViewModel Cart, OrderViewModel OrderModel)
        {
            var user = await _UserManager.FindByNameAsync(UserName);
            if (user is null)
                throw new InvalidOperationException($"Пользователь с именем {UserName} в БД отсутствует");

            await using var transaction = await _db.Database.BeginTransactionAsync();

            var order = new Order
            {
                Name = OrderModel.Name,
                Address = OrderModel.Address,
                Phone = OrderModel.Phone,
                User = user
            };

            var product_ids = Cart.Items.Select(item => item.Product.Id).ToArray();

            var cart_products = await _db.Products
              .Where(p => product_ids.Contains(p.Id))
              .ToArrayAsync();

            order.Items = Cart.Items.Join(
                cart_products,
                cart_item => cart_item.Product.Id,
                product => product.Id,
                (cart_item, product) => new OrderItem
                {
                    Order = order,
                    Product = product,
                    Price = product.Price,  // место, где могут быть применены скидки
                    Quantity = cart_item.Quantity,
                }).ToArray();

            await _db.Orders.AddAsync(order);
            await _db.SaveChangesAsync();

            await transaction.CommitAsync();

            return order;
        }
    }
}

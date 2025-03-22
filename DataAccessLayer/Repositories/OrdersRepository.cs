using DataAccessLayer.Entities;
using DataAccessLayer.RepositoryContracts;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Repositories
{
    public class OrdersRepository : IOrdersRepository
    {
        private readonly IMongoCollection<Order> _orders;
        private readonly string collectionName = "orders";
        public OrdersRepository(IMongoDatabase mongoDatabase)
        {
            _orders = mongoDatabase.GetCollection<Order>(collectionName);
        }
        public async Task<Order?> AddOrder(Order order)
        {
            order.OrderID = Guid.NewGuid();
            await _orders.InsertOneAsync(order);
            return order;

        }

        public async Task<bool> DeleteOrder(Guid orderID)
        {
            FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(temp=> temp.OrderID, orderID);

            Order? existingOrder = await _orders.Find(filter).FirstOrDefaultAsync();

            if (existingOrder != null) 
            {
                return false;
            }
            DeleteResult deleteResult = await _orders.DeleteOneAsync(filter);
            return deleteResult.DeletedCount > 0;
        }

        public async Task<IEnumerable<Order?>> GetOrderByCondition(FilterDefinition<Order> filter)
        {
            return (IEnumerable<Order?>)(await _orders.FindAsync(filter)).FirstOrDefault();
        }

        public async Task<IEnumerable<Order?>> GetOrders()
        {
            return await _orders.Find(_ => true).ToListAsync();
        }

        public async Task<IEnumerable<Order?>> GetOrdersByCondition(FilterDefinition<Order> filter)
        {
            return (IEnumerable<Order?>)(await _orders.FindAsync(filter)).ToListAsync();
        }

        public async Task<Order?> UpdateOrder(Order order)
        {
            FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(temp => temp.OrderID, order.OrderID);

            Order? existingOrder = await _orders.Find(filter).FirstOrDefaultAsync();

            if (existingOrder == null)
            {
                return null;
            }
            ReplaceOneResult replaceOneResult= await _orders.ReplaceOneAsync(filter, order);
            return replaceOneResult.IsAcknowledged ? order : null;
        }
    }
}

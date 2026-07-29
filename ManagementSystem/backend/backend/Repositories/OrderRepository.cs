using backend.Configuration;
using backend.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace backend.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly IMongoCollection<Order> _orders;

    public OrderRepository(
        IMongoClient client,
        IOptions<MongoDbSettings> settings)
    {
        var database = client.GetDatabase(settings.Value.DatabaseName);

        _orders = database.GetCollection<Order>("Orders");
    }

    public async Task CreateAsync(Order order)
    {
        await _orders.InsertOneAsync(order);
    }

    public async Task<Order?> GetByIdAsync(string id)
    {
        return await _orders
            .Find(x => x.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Order>> GetByCustomerIdAsync(string customerId)
    {
        return await _orders
            .Find(x => x.CustomerId == customerId)
            .SortByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Order>> GetByShopIdAsync(string shopId)
    {
        return await _orders
            .Find(x => x.ShopId == shopId)
            .SortByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> GetNextQueueNumberAsync(string shopId)
    {
        var lastOrder = await _orders
            .Find(x => x.ShopId == shopId)
            .SortByDescending(x => x.QueueNumber)
            .FirstOrDefaultAsync();

        if (lastOrder == null)
            return 1;

        return lastOrder.QueueNumber + 1;
    }

        public async Task<int> GetPendingCountAsync(string shopId)
    {
            // Count orders that are pending or preparing
            var filter = Builders<Order>.Filter.And(
                Builders<Order>.Filter.Eq(x => x.ShopId, shopId),
                Builders<Order>.Filter.In(x => x.Status, new[] { OrderStatus.Pending, OrderStatus.Preparing })
            );

            var count = await _orders.CountDocumentsAsync(filter);
            return (int)count;
        }

        public async Task UpdateAsync(Order order)
        {
            await _orders.ReplaceOneAsync(
                x => x.Id == order.Id,
                order);
        }
    }
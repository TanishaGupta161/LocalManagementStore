using backend.Models;

namespace backend.Repositories;

public interface IOrderRepository
{
    Task CreateAsync(Order order);

    Task<Order?> GetByIdAsync(string id);

    Task<List<Order>> GetByCustomerIdAsync(string customerId);

    Task<List<Order>> GetByShopIdAsync(string shopId);

    Task<int> GetNextQueueNumberAsync(string shopId);

    Task UpdateAsync(Order order);
}
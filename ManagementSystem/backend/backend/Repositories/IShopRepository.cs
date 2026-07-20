using backend.Models;

namespace backend.Repositories;

public interface IShopRepository
{
    Task CreateAsync(Shop shop);

    Task<Shop?> GetByIdAsync(string id);

    Task<Shop?> GetByOwnerIdAsync(string ownerId);

    Task<List<Shop>> GetAllAsync();

    Task UpdateAsync(Shop shop);

    Task DeleteAsync(string id);
}
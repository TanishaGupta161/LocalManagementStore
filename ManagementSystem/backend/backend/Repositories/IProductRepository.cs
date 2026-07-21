using backend.Models;

namespace backend.Repositories;

public interface IProductRepository
{
    Task CreateAsync(Product product);

    Task<List<Product>> GetByShopIdAsync(string shopId);

    Task<Product?> GetByIdAsync(string id);

    Task UpdateAsync(Product product);

    Task DeleteAsync(string id);
    Task<List<Product>> GetProductsByIdsAsync(List<string> productIds);
}
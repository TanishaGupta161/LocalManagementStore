using backend.Configuration;
using backend.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace backend.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly IMongoCollection<Product> _products;

    public ProductRepository(
        IMongoClient client,
        IOptions<MongoDbSettings> settings)
    {
        var database = client.GetDatabase(settings.Value.DatabaseName);

        _products = database.GetCollection<Product>("Products");
    }

    public async Task CreateAsync(Product product)
    {
        await _products.InsertOneAsync(product);
    }

    public async Task<List<Product>> GetByShopIdAsync(string shopId)
    {
        return await _products.Find(x => x.ShopId == shopId).ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(string id)
    {
        return await _products.Find(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(Product product)
    {
        await _products.ReplaceOneAsync(x => x.Id == product.Id, product);
    }

    public async Task DeleteAsync(string id)
    {
        await _products.DeleteOneAsync(x => x.Id == id);
    }
    public async Task<List<Product>> GetProductsByIdsAsync(List<string> productIds)
{
    return await _products
        .Find(x => productIds.Contains(x.Id!))
        .ToListAsync();
}
public async Task CreateManyAsync(List<Product> products)
{
    await _products.InsertManyAsync(products);
}
}
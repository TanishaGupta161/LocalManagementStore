using backend.Configuration;
using backend.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace backend.Repositories;

public class ShopRepository : IShopRepository
{
    private readonly IMongoCollection<Shop> _shops;

    public ShopRepository(
        IMongoClient mongoClient,
        IOptions<MongoDbSettings> settings)
    {
        var database = mongoClient.GetDatabase(settings.Value.DatabaseName);

        _shops = database.GetCollection<Shop>("Shops");
    }

    public async Task CreateAsync(Shop shop)
    {
        await _shops.InsertOneAsync(shop);
    }

    public async Task<Shop?> GetByIdAsync(string id)
    {
        return await _shops.Find(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task<Shop?> GetByOwnerIdAsync(string ownerId)
    {
        return await _shops.Find(x => x.OwnerId == ownerId).FirstOrDefaultAsync();
    }

    public async Task<List<Shop>> GetAllAsync()
    {
        return await _shops.Find(_ => true).ToListAsync();
    }

    public async Task UpdateAsync(Shop shop)
    {
        await _shops.ReplaceOneAsync(x => x.Id == shop.Id, shop);
    }

    public async Task DeleteAsync(string id)
    {
        await _shops.DeleteOneAsync(x => x.Id == id);
    }
}
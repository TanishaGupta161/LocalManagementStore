using backend.Configuration;
using backend.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace backend.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _users;

    public UserRepository(
        IMongoClient mongoClient,
        IOptions<MongoDbSettings> settings)
    {
        var database = mongoClient.GetDatabase(settings.Value.DatabaseName);

        _users = database.GetCollection<User>("Users");
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _users.Find(x => x.Email == email)
                           .FirstOrDefaultAsync();
    }

    public async Task<User?> GetByIdAsync(string id)
    {
        return await _users.Find(x => x.Id == id)
                           .FirstOrDefaultAsync();
    }

    public async Task CreateAsync(User user)
    {
        await _users.InsertOneAsync(user);
    }

    public async Task UpdateAsync(User user)
    {
        await _users.ReplaceOneAsync(x => x.Id == user.Id, user);
    }

    public async Task DeleteAsync(string id)
    {
        await _users.DeleteOneAsync(x => x.Id == id);
    }
}
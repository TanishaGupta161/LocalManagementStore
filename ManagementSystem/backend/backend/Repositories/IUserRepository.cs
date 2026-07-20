using backend.Models;

namespace backend.Repositories;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);

    Task<User?> GetByIdAsync(string id);

    Task CreateAsync(User user);

    Task UpdateAsync(User user);

    Task DeleteAsync(string id);
}
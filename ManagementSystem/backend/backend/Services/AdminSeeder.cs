using backend.Models;
using backend.Models.Enums;
using backend.Repositories;

namespace backend.Services;

public class AdminSeeder
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;

    public AdminSeeder(
        IUserRepository userRepository,
        IPasswordService passwordService)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
    }

    public async Task SeedAdminAsync()
    {
        var admin = await _userRepository.GetByEmailAsync("admin@gmail.com");

        if (admin != null)
            return;

     var defaultAdmin = new User
{
    Name = "Super Admin",
    Email = "admin@gmail.com",
    Phone = "9999999999",
    PasswordHash = _passwordService.HashPassword("Admin@123"),
    Role = UserRole.Admin,
    IsActive = true,
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow
};

        await _userRepository.CreateAsync(defaultAdmin);
    }
}
using backend.DTOs.Admin;
using backend.Models;
using backend.Models.Enums;
using backend.Repositories;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;

    public AdminController(
        IUserRepository userRepository,
        IPasswordService passwordService)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
    }

    [HttpPost("create-shopkeeper")]
    public async Task<IActionResult> CreateShopkeeper(
        [FromBody] CreateShopkeeperRequest request)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);

        if (existingUser != null)
        {
            return BadRequest(new
            {
                Success = false,
                Message = "Email already exists."
            });
        }

        var shopkeeper = new User
        {
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            PasswordHash = _passwordService.HashPassword(request.Password),
            Role = UserRole.Shopkeeper,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(shopkeeper);

        return Ok(new
        {
            Success = true,
            Message = "Shopkeeper created successfully."
        });
    }
}
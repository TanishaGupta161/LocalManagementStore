using backend.DTOs.Shop;
using backend.Models;
using backend.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Shopkeeper")]
public class ShopController : ControllerBase
{
    private readonly IShopRepository _shopRepository;

    public ShopController(IShopRepository shopRepository)
    {
        _shopRepository = shopRepository;
    }

    [HttpPost]
    public async Task<IActionResult> CreateShop([FromBody] CreateShopRequest request)
    {
        // Logged-in Shopkeeper Id from JWT
        var ownerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(ownerId))
        {
            return Unauthorized(new
            {
                Success = false,
                Message = "Invalid token."
            });
        }

        // Check if shop already exists
        var existingShop = await _shopRepository.GetByOwnerIdAsync(ownerId);

        if (existingShop != null)
        {
            return BadRequest(new
            {
                Success = false,
                Message = "You already have a shop."
            });
        }

        var shop = new Shop
        {
            ShopName = request.ShopName,
            Address = request.Address,
            Category = request.Category,
            OwnerId = ownerId,
                    // Set coordinates if provided
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    IsOpen = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

        await _shopRepository.CreateAsync(shop);

        return Ok(new
        {
            Success = true,
            Message = "Shop created successfully.",
            Shop = shop
        });
    }
    [HttpGet]
[AllowAnonymous]
public async Task<IActionResult> GetAllShops()
{
    var shops = await _shopRepository.GetAllAsync();

    return Ok(new
    {
        Success = true,
        Count = shops.Count,
        Shops = shops
    });
}
[HttpGet("{id}")]
[AllowAnonymous]
public async Task<IActionResult> GetShopById(string id)
{
    var shop = await _shopRepository.GetByIdAsync(id);

    if (shop == null)
    {
        return NotFound(new
        {
            Success = false,
            Message = "Shop not found."
        });
    }

    return Ok(new
    {
        Success = true,
        Shop = shop
    });
}
}
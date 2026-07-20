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
}
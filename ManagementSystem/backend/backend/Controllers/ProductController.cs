using backend.DTOs.Product;
using backend.Models;
using backend.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Shopkeeper")]
public class ProductController : ControllerBase
{
    private readonly IProductRepository _productRepository;
    private readonly IShopRepository _shopRepository;

    public ProductController(
        IProductRepository productRepository,
        IShopRepository shopRepository)
    {
        _productRepository = productRepository;
        _shopRepository = shopRepository;
    }

    [HttpPost]
    public async Task<IActionResult> AddProduct([FromBody] CreateProductRequest request)
    {
        // Logged in Shopkeeper Id
        var ownerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(ownerId))
        {
            return Unauthorized(new
            {
                Success = false,
                Message = "Invalid token."
            });
        }

        // Find shop of logged in shopkeeper
        var shop = await _shopRepository.GetByOwnerIdAsync(ownerId);

        if (shop == null)
        {
            return BadRequest(new
            {
                Success = false,
                Message = "Please create your shop first."
            });
        }

        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Stock = request.Stock,
            Category = request.Category,

            // Automatically attach shop
            ShopId = shop.Id!,

            IsAvailable = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _productRepository.CreateAsync(product);

        return Ok(new
        {
            Success = true,
            Message = "Product added successfully.",
            Product = product
        });
    }

    [HttpGet("my-products")]
    public async Task<IActionResult> GetMyProducts()
    {
        var ownerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(ownerId))
        {
            return Unauthorized();
        }

        var shop = await _shopRepository.GetByOwnerIdAsync(ownerId);

        if (shop == null)
        {
            return BadRequest(new
            {
                Success = false,
                Message = "Shop not found."
            });
        }

        var products = await _productRepository.GetByShopIdAsync(shop.Id!);

        return Ok(products);
    }
}
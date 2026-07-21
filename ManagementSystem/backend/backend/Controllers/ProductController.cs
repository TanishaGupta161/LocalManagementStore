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

    // Add Product
    [HttpPost]
    public async Task<IActionResult> AddProduct([FromBody] CreateProductRequest request)
    {
        var ownerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(ownerId))
        {
            return Unauthorized(new
            {
                Success = false,
                Message = "Invalid token."
            });
        }

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

    // Get My Products
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

        return Ok(new
        {
            Success = true,
            Products = products
        });
    }

    // Get Product By Id
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById(string id)
    {
        var product = await _productRepository.GetByIdAsync(id);

        if (product == null)
        {
            return NotFound(new
            {
                Success = false,
                Message = "Product not found."
            });
        }

        return Ok(new
        {
            Success = true,
            Product = product
        });
    }

    // Update Product
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(string id, [FromBody] UpdateProductRequest request)
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

        var product = await _productRepository.GetByIdAsync(id);

        if (product == null)
        {
            return NotFound(new
            {
                Success = false,
                Message = "Product not found."
            });
        }

        if (product.ShopId != shop.Id)
        {
            return Forbid();
        }

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.Stock = request.Stock;
        product.Category = request.Category;
        product.IsAvailable = request.IsAvailable;
        product.UpdatedAt = DateTime.UtcNow;

        await _productRepository.UpdateAsync(product);

        return Ok(new
        {
            Success = true,
            Message = "Product updated successfully.",
            Product = product
        });
    }

    // Delete Product
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(string id)
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

        var product = await _productRepository.GetByIdAsync(id);

        if (product == null)
        {
            return NotFound(new
            {
                Success = false,
                Message = "Product not found."
            });
        }

        if (product.ShopId != shop.Id)
        {
            return Forbid();
        }

        await _productRepository.DeleteAsync(id);

        return Ok(new
        {
            Success = true,
            Message = "Product deleted successfully."
        });
    }
}
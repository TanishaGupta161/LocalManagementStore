using backend.DTOs.Product;
using backend.Models;
using backend.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ClosedXML.Excel;

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
                    ShopName = shop.ShopName,
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
                    ShopName = shop.ShopName,
                    Products = products
                });
    }

    [HttpPost("upload-excel")]
    public async Task<IActionResult> UploadExcel([FromForm] UploadExcelRequest request)
    {
        var ownerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(ownerId)) return Unauthorized();
        var shop = await _shopRepository.GetByOwnerIdAsync(ownerId);
        if (shop == null) return BadRequest(new { Success = false, Message = "Please create your shop first." });
        if (request.File == null || request.File.Length == 0) return BadRequest(new { Success = false, Message = "Please select an Excel file." });
        if (!Path.GetExtension(request.File.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase)) return BadRequest(new { Success = false, Message = "Only .xlsx Excel files are supported." });

        var products = new List<Product>();
        try
        {
            using var stream = request.File.OpenReadStream();
            using var workbook = new XLWorkbook(stream);
            var sheet = workbook.Worksheets.FirstOrDefault();
            if (sheet == null) return BadRequest(new { Success = false, Message = "The workbook does not contain a worksheet." });
            var header = sheet.FirstRowUsed();
            if (header == null) return BadRequest(new { Success = false, Message = "The worksheet is empty." });
            var columns = header.CellsUsed().ToDictionary(cell => cell.GetString().Trim(), cell => cell.Address.ColumnNumber, StringComparer.OrdinalIgnoreCase);
            var required = new[] { "Name", "Description", "Price", "Stock", "Category" };
            if (required.Any(name => !columns.ContainsKey(name))) return BadRequest(new { Success = false, Message = "Headers required: Name, Description, Price, Stock, Category." });
            foreach (var row in sheet.RowsUsed().Skip(1))
            {
                var name = row.Cell(columns["Name"]).GetString().Trim();
                if (string.IsNullOrWhiteSpace(name)) continue;
                if (!decimal.TryParse(row.Cell(columns["Price"]).GetString(), out var price) || price < 0 || !int.TryParse(row.Cell(columns["Stock"]).GetString(), out var stock) || stock < 0) return BadRequest(new { Success = false, Message = $"Row {row.RowNumber()} has an invalid Price or Stock." });
                products.Add(new Product { Name = name, Description = row.Cell(columns["Description"]).GetString().Trim(), Price = price, Stock = stock, Category = row.Cell(columns["Category"]).GetString().Trim(), ShopId = shop.Id!, IsAvailable = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
            }
        }
        catch (Exception) { return BadRequest(new { Success = false, Message = "We could not read this Excel file. Please use a valid .xlsx file." }); }
        if (products.Count == 0) return BadRequest(new { Success = false, Message = "No product rows were found in the file." });
        await _productRepository.CreateManyAsync(products);
        return Ok(new { Success = true, Message = $"{products.Count} products imported successfully.", Count = products.Count });
    }

    // Get Product By Id
    [HttpGet("{id}")]
    [AllowAnonymous]
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

        var shop = await _shopRepository.GetByIdAsync(product.ShopId);

                return Ok(new
                {
                    Success = true,
                    ShopName = shop?.ShopName,
                    Product = product
                });
    }

    // Public products for a selected shop (used by customers to browse and order)
    [HttpGet("shop/{shopId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProductsByShop(string shopId)
    {
        var shop = await _shopRepository.GetByIdAsync(shopId);

        if (shop == null)
        {
            return NotFound(new { Success = false, Message = "Shop not found." });
        }

        var products = await _productRepository.GetByShopIdAsync(shopId);

        return Ok(new
        {
            Success = true,
            ShopName = shop.ShopName,
            Products = products.Where(product => product.IsAvailable).ToList()
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
                    ShopName = shop.ShopName,
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

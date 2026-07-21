using backend.DTOs.Order;
using backend.Models;
using backend.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public partial class OrderController : ControllerBase
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly IShopRepository _shopRepository;
    private readonly IProductRepository _productRepository;

    public OrderController(
        IOrderRepository orderRepository,
        IUserRepository userRepository,
        IShopRepository shopRepository,
        IProductRepository productRepository)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _shopRepository = shopRepository;
        _productRepository = productRepository;
    }
        [HttpPost]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> PlaceOrder(CreateOrderRequest request)
    {
        var customerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(customerId))
        {
            return Unauthorized(new
            {
                Success = false,
                Message = "Invalid token."
            });
        }

        var customer = await _userRepository.GetByIdAsync(customerId);

        if (customer == null)
        {
            return Unauthorized(new
            {
                Success = false,
                Message = "Customer not found."
            });
        }

        var shop = await _shopRepository.GetByIdAsync(request.ShopId);

        if (shop == null)
        {
            return NotFound(new
            {
                Success = false,
                Message = "Shop not found."
            });
        }

        if (request.Items == null || request.Items.Count == 0)
        {
            return BadRequest(new
            {
                Success = false,
                Message = "Please select at least one product."
            });
        }

        var orderItems = new List<OrderItem>();

        decimal totalAmount = 0;

        foreach (var item in request.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId);

            if (product == null)
            {
                return NotFound(new
                {
                    Success = false,
                    Message = $"Product not found : {item.ProductId}"
                });
            }

            if (product.ShopId != request.ShopId)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "Product does not belong to selected shop."
                });
            }

            if (product.Stock < item.Quantity)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = $"Insufficient stock for {product.Name}"
                });
            }

            orderItems.Add(new OrderItem
            {
                ProductId = product.Id!,
                ProductName = product.Name,
                Price = product.Price,
                Quantity = item.Quantity
            });

            totalAmount += product.Price * item.Quantity;
        }

        int queueNumber =
            await _orderRepository.GetNextQueueNumberAsync(request.ShopId);

        var order = new Order
        {
            CustomerId = customerId,
            ShopId = request.ShopId,
            Items = orderItems,
            TotalAmount = totalAmount,
            QueueNumber = queueNumber,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _orderRepository.CreateAsync(order);

        return Ok(new
        {
            Success = true,
            Message = "Order placed successfully.",
            QueueNumber = queueNumber,
            Order = order
        });
    }
    [HttpGet("my-orders")]
[Authorize(Roles = "Customer")]
public async Task<IActionResult> GetMyOrders()
{
    var customerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    if (string.IsNullOrEmpty(customerId))
    {
        return Unauthorized(new
        {
            Success = false,
            Message = "Invalid token."
        });
    }

    var orders = await _orderRepository.GetByCustomerIdAsync(customerId);

    return Ok(new
    {
        Success = true,
        Count = orders.Count,
        Orders = orders
    });
}
[HttpGet("shop-orders")]
[Authorize(Roles = "Shopkeeper")]
public async Task<IActionResult> GetShopOrders()
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
            Message = "Shop not found."
        });
    }

    var orders = await _orderRepository.GetByShopIdAsync(shop.Id!);

    return Ok(new
    {
        Success = true,
        Count = orders.Count,
        Orders = orders
    });
}
[HttpPut("{id}/status")]
[Authorize(Roles = "Shopkeeper")]
public async Task<IActionResult> UpdateOrderStatus(
    string id,
    [FromBody] UpdateOrderStatusRequest request)
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
            Message = "Shop not found."
        });
    }

    var order = await _orderRepository.GetByIdAsync(id);

    if (order == null)
    {
        return NotFound(new
        {
            Success = false,
            Message = "Order not found."
        });
    }

    // Security Check
    if (order.ShopId != shop.Id)
    {
        return Forbid();
    }

    order.Status = request.Status;
    order.UpdatedAt = DateTime.UtcNow;

    await _orderRepository.UpdateAsync(order);

    return Ok(new
    {
        Success = true,
        Message = "Order status updated successfully.",
        Order = order
    });
}
}
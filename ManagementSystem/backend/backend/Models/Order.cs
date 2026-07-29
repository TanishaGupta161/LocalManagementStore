using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Models;

public class Order
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string CustomerId { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.ObjectId)]
    public string ShopId { get; set; } = string.Empty;

    public List<OrderItem> Items { get; set; } = new();

    public decimal TotalAmount { get; set; }

    public int QueueNumber { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    // Distance from customer to shop (kilometers)
    public double DistanceKm { get; set; } = 0.0;

    // Estimated ready time computed at order creation
    public DateTime EstimatedReadyAt { get; set; } = DateTime.UtcNow;

    // Estimated ready in minutes (convenience field)
    public int EstimatedReadyInMinutes { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
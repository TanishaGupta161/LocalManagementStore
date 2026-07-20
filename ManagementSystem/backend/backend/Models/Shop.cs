using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Models;

public class Shop
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string ShopName { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string OwnerId { get; set; } = string.Empty;

    public bool IsOpen { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
namespace backend.DTOs.Order;

public class CreateOrderRequest
{
    public string ShopId { get; set; } = string.Empty;

    public List<CreateOrderItemRequest> Items { get; set; } = new();

    // Customer location (optional) to compute distance and ETA
    public double CustomerLatitude { get; set; } = 0.0;
    public double CustomerLongitude { get; set; } = 0.0;
}

public class CreateOrderItemRequest
{
    public string ProductId { get; set; } = string.Empty;

    public int Quantity { get; set; }
}
namespace backend.DTOs.Shop;

public class CreateShopRequest
{
    public string ShopName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}
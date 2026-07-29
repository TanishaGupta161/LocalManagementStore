namespace backend.DTOs.Shop;

public class CreateShopRequest
{
    public string ShopName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;

    // Optional coordinates for the shop (latitude, longitude)
    public double Latitude { get; set; } = 0.0;
    public double Longitude { get; set; } = 0.0;
}
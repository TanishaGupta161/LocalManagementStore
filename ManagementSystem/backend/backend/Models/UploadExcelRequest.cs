namespace backend.DTOs.Product;

public class UploadExcelRequest
{
    public IFormFile File { get; set; } = default!;
}
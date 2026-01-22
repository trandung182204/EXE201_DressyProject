namespace BE.DTOs;

public class ProductListItemDto
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public string? CategoryName { get; set; }
    public string? ThumbnailUrl { get; set; }

    // Giá thuê/ngày thấp nhất trong variants
    public decimal? MinPricePerDay { get; set; }

    public string? Status { get; set; }
}

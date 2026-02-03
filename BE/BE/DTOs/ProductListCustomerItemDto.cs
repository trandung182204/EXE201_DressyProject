namespace BE.DTOs;

public class ProductListCustomerItemDto
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public string? CategoryName { get; set; }
    public string? ThumbnailUrl { get; set; }
    public decimal? MinPricePerDay { get; set; }
    public DateTime? CreatedAt { get; set; }

    // để filter UI dễ hơn (nếu muốn)
    public List<string> Sizes { get; set; } = new();
    public List<string> Colors { get; set; } = new();
}
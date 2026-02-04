namespace BE.DTOs;

public class ProductListItemDto
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public long? CategoryId { get; set; }
    public string? CategoryName { get; set; }

    // CHANGED
    public long? ThumbnailFileId { get; set; }

    // OPTIONAL
    public string? ThumbnailUrl { get; set; }

    public decimal? MinPricePerDay { get; set; }
    public string? Status { get; set; }
}

namespace BE.DTOs;

public class ProductListQuery
{
    public string? Status { get; set; } = "AVAILABLE";

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 9;

    // sortBy: createdAt | name | price
    public string? SortBy { get; set; } = "createdAt";
    // sortDir: asc | desc
    public string? SortDir { get; set; } = "desc";

    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }

    // categoryIds=1,2,3
    public List<long>? CategoryIds { get; set; }

    // size=S,M ; color=Red,Black
    public List<string>? Sizes { get; set; }
    public List<string>? Colors { get; set; }

    // chỉ lấy variant còn hàng
    public bool OnlyAvailable { get; set; } = true;
}
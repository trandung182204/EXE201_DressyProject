namespace BE.DTOs.Cart;

public class CartItemDetailDto
{
    public long CartItemId { get; set; }

    public long ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public string? ImageUrl { get; set; }

    public long ProductVariantId { get; set; }
    public string? SizeLabel { get; set; }
    public string? ColorName { get; set; }

    public int Quantity { get; set; }

    public decimal PricePerDay { get; set; }
    public decimal DepositAmount { get; set; }

    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    public int RentalDays { get; set; }
    public decimal TotalPrice { get; set; }
}

using BE.DTOs.Cart;

namespace BE.DTOs;

public class AddToCartDto
{
    public long ProductVariantId { get; set; }
    public int Quantity { get; set; } = 1;
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}

public class CartDetailDto
{
    public long CartId { get; set; }
    public List<CartItemDetailDto> Items { get; set; } = new();
}

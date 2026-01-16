namespace BE.DTOs;

public class UpdateProductVariantDto
{
    public long? Id { get; set; } // null = tạo mới
    public string SizeLabel { get; set; } = null!;
    public string ColorName { get; set; } = null!;
    public string ColorCode { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal PricePerDay { get; set; }
    public decimal DepositAmount { get; set; }
    public bool Status { get; set; }
}

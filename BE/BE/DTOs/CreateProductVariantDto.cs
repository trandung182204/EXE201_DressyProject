namespace BE.DTOs;

public class CreateProductVariantDto
{
    public string? SizeLabel { get; set; }
    public string? ColorName { get; set; }
    public string? ColorCode { get; set; }
    public int Quantity { get; set; }
    public decimal PricePerDay { get; set; }
    public decimal DepositAmount { get; set; }
}


namespace BE.DTOs;

public class UpdateProductVariantDto
{
    public long? Id { get; set; }
    public string? SizeLabel { get; set; }
    public string? ColorName { get; set; }
    public string? ColorCode { get; set; }

    public int? Quantity { get; set; }
    public decimal? PricePerDay { get; set; }
    public decimal? DepositAmount { get; set; }
    public bool? Status { get; set; }
}


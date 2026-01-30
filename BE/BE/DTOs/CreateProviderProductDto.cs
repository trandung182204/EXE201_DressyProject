using System.Collections.Generic;

namespace BE.DTOs
{
    public class CreateProviderProductDto
    {
        public long CategoryId { get; set; }
        public string Name { get; set; } = "";
        public string ProductType { get; set; } = "";
        public string? Description { get; set; }

        public List<string> ImageUrls { get; set; } = new();
        public List<CreateProviderProductVariantDto> Variants { get; set; } = new();
    }

    public class CreateProviderProductVariantDto
    {
        public string? SizeLabel { get; set; }
        public string? ColorName { get; set; }
        public string? ColorCode { get; set; }
        public int Quantity { get; set; }

        public decimal PricePerDay { get; set; }
        public decimal DepositAmount { get; set; }

        // nếu entity ProductVariants của bạn có Status (bool) như code cũ
        public bool Status { get; set; } = true;
    }
}

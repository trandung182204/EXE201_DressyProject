namespace BE.DTOs
{
    public class ProductDetailDto
    {
        public long? Id { get; set; }
        public long? ProviderId { get; set; }
        public long? CategoryId { get; set; }
        public string? CategoryName { get; set; }

        public string? Name { get; set; }
        public string? ProductType { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }

        // CHANGED: trả id file (DB không lưu link nữa)
        public List<long?> ImageFileIds { get; set; } = new();

        // OPTIONAL: FE vẫn cần hiển thị ảnh -> BE sinh URL runtime (không lưu DB)
        public List<string?> ImageUrls { get; set; } = new();

        public List<ProductVariantDetailDto> Variants { get; set; } = new();
    }

    public class ProductVariantDetailDto
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
}

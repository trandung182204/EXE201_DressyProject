namespace BE.DTOs
{
    public class UpdateProviderProductDto
    {
        public long CategoryId { get; set; }
        public string? Name { get; set; }
        public string? ProductType { get; set; }  // "OUTFIT"...
        public string? Description { get; set; }
        public string? Status { get; set; }       // "AVAILABLE"/"UNAVAILABLE"

        public List<string?> ImageUrls { get; set; } = new();
        public List<UpdateProductVariantDto> Variants { get; set; } = new();
    }

    
}

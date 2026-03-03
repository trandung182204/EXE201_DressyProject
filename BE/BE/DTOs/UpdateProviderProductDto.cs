using System.Collections.Generic;

namespace BE.DTOs
{
    public class UpdateProviderProductDto
    {
        public long CategoryId { get; set; }
        public string? Name { get; set; }
        public string? ProductType { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }

        // CHANGED
        public List<long> ImageFileIds { get; set; } = new();

        public List<UpdateProductVariantDto> Variants { get; set; } = new();
    }
}

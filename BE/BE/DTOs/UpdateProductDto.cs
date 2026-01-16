using System;
using System.Collections.Generic;

namespace BE.DTOs
{
    public class UpdateProductDto
{
    // PRODUCTS
    public long CategoryId { get; set; }
    public string Name { get; set; } = null!;
    public string ProductType { get; set; } = "OUTFIT";
    public string? Description { get; set; }
    public string Status { get; set; } = "AVAILABLE";

    // IMAGES
    public List<string>? ImageUrls { get; set; }

    // VARIANTS
    public List<UpdateProductVariantDto>? Variants { get; set; }
}

}




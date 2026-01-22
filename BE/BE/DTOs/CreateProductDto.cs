using System;
using System.Collections.Generic;

namespace BE.DTOs;

public class CreateProductDto
{
    public long CategoryId { get; set; }
    public string Name { get; set; } = "";
    public string ProductType { get; set; } = ""; // OUTFIT | MAKEUP | PHOTOGRAPHY
    public string? Description { get; set; }

    // 🔹 Ảnh sản phẩm
    public List<string> ImageUrls { get; set; } = new();

    // 🔹 Variants
    public List<CreateProductVariantDto> Variants { get; set; } = new();
}


using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class ProductImages
{
    public long Id { get; set; }

    public long? ProductId { get; set; }

    public string? ImageUrl { get; set; }

    public virtual Products? Product { get; set; }
}

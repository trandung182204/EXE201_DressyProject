using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class CartItems
{
    public long Id { get; set; }

    public long? CartId { get; set; }

    public long? ProductVariantId { get; set; }

    public int? Quantity { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public virtual Carts? Cart { get; set; }

    public virtual ProductVariants? ProductVariant { get; set; }
}

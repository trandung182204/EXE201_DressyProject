using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class ProductReviews
{
    public long Id { get; set; }

    public long? ProductId { get; set; }

    public long? CustomerId { get; set; }

    public long? BookingItemId { get; set; }

    public int? Rating { get; set; }

    public string? Comment { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual BookingItems? BookingItem { get; set; }

    public virtual Users? Customer { get; set; }

    public virtual Products? Product { get; set; }
}

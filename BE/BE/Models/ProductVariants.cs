using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class ProductVariants
{
    public long Id { get; set; }

    public long? ProductId { get; set; }

    public string? SizeLabel { get; set; }

    public string? ColorName { get; set; }

    public string? ColorCode { get; set; }

    public int? Quantity { get; set; }

    public decimal? PricePerDay { get; set; }

    public decimal? DepositAmount { get; set; }

    public bool? Status { get; set; }

    public virtual ICollection<BookingItems> BookingItems { get; set; } = new List<BookingItems>();

    public virtual ICollection<CartItems> CartItems { get; set; } = new List<CartItems>();

    public virtual Products? Product { get; set; }
}

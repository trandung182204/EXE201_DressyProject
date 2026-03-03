using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class ProductRatingSummary
{
    public long ProductId { get; set; }

    public decimal? AverageRating { get; set; }

    public int? TotalReviews { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Products Product { get; set; } = null!;
}

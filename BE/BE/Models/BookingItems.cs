using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class BookingItems
{
    public long Id { get; set; }

    public long? BookingId { get; set; }

    public long? ProductId { get; set; }

    public long? ProductVariantId { get; set; }

    public long? ProviderId { get; set; }

    public long? ProviderBranchId { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public decimal? Price { get; set; }

    public decimal? CommissionAmount { get; set; }

    public virtual Bookings? Booking { get; set; }

    public virtual ICollection<ProductReviews> ProductReviews { get; set; } = new List<ProductReviews>();

    public virtual ProductVariants? ProductVariant { get; set; }

    public virtual ProviderBranches? ProviderBranch { get; set; }

    public virtual ICollection<ProviderEarnings> ProviderEarnings { get; set; } = new List<ProviderEarnings>();
}

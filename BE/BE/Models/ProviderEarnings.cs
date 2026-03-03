using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class ProviderEarnings
{
    public long Id { get; set; }

    public long? ProviderId { get; set; }

    public long? BookingItemId { get; set; }

    public long? ProviderBranchId { get; set; }

    public decimal? GrossAmount { get; set; }

    public decimal? Commission { get; set; }

    public decimal? NetAmount { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual BookingItems? BookingItem { get; set; }

    public virtual Providers? Provider { get; set; }

    public virtual ProviderBranches? ProviderBranch { get; set; }
}

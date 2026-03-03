using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class ProviderReports
{
    public long Id { get; set; }

    public long? ProviderId { get; set; }

    public long? ProviderBranchId { get; set; }

    public string? Month { get; set; }

    public int? TotalBookings { get; set; }

    public decimal? TotalRevenue { get; set; }

    public int? RepeatCustomers { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Providers? Provider { get; set; }

    public virtual ProviderBranches? ProviderBranch { get; set; }
}

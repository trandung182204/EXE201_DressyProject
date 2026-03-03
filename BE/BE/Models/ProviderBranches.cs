using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class ProviderBranches
{
    public long Id { get; set; }

    public long? ProviderId { get; set; }

    public string? BranchName { get; set; }

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public bool? IsMainBranch { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<BookingItems> BookingItems { get; set; } = new List<BookingItems>();

    public virtual Providers? Provider { get; set; }

    public virtual ICollection<ProviderEarnings> ProviderEarnings { get; set; } = new List<ProviderEarnings>();

    public virtual ICollection<ProviderFeedbacks> ProviderFeedbacks { get; set; } = new List<ProviderFeedbacks>();

    public virtual ICollection<ProviderReports> ProviderReports { get; set; } = new List<ProviderReports>();
}

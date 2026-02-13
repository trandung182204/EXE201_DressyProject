using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class Providers
{
    public long Id { get; set; }

    public long? UserId { get; set; }

    public string? BrandName { get; set; }
    public string? Description { get; set; }

    public string? ProviderType { get; set; }

    public bool? Verified { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
    public long? LogoFileId { get; set; }
    public DateTimeOffset? LastNotificationSeenAt { get; set; }

    public virtual MediaFiles? LogoFile { get; set; }

    public virtual ICollection<Products> Products { get; set; } = new List<Products>();

    public virtual ICollection<ProviderBranches> ProviderBranches { get; set; } = new List<ProviderBranches>();

    public virtual ICollection<ProviderEarnings> ProviderEarnings { get; set; } = new List<ProviderEarnings>();

    public virtual ICollection<ProviderFeedbacks> ProviderFeedbacks { get; set; } = new List<ProviderFeedbacks>();

    public virtual ICollection<ProviderReports> ProviderReports { get; set; } = new List<ProviderReports>();

    public virtual ICollection<ProviderSubscriptions> ProviderSubscriptions { get; set; } = new List<ProviderSubscriptions>();

    public virtual Users? User { get; set; }
}

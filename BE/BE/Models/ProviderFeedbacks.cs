using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class ProviderFeedbacks
{
    public long Id { get; set; }

    public long? ProviderId { get; set; }

    public long? ProviderBranchId { get; set; }

    public long? CustomerId { get; set; }

    public long? BookingId { get; set; }

    public string? FeedbackType { get; set; }

    public string? Content { get; set; }

    public string? AttachmentUrl { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Bookings? Booking { get; set; }

    public virtual Users? Customer { get; set; }

    public virtual ICollection<FeedbackResponses> FeedbackResponses { get; set; } = new List<FeedbackResponses>();

    public virtual Providers? Provider { get; set; }

    public virtual ProviderBranches? ProviderBranch { get; set; }
}

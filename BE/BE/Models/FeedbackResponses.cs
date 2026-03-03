using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class FeedbackResponses
{
    public long Id { get; set; }

    public long? FeedbackId { get; set; }

    public long? AdminId { get; set; }

    public string? ResponseContent { get; set; }

    public string? ActionTaken { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Users? Admin { get; set; }

    public virtual ProviderFeedbacks? Feedback { get; set; }
}

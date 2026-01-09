using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class ProviderSubscriptions
{
    public long Id { get; set; }

    public long? ProviderId { get; set; }

    public int? PlanId { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string? Status { get; set; }

    public virtual SubscriptionPlans? Plan { get; set; }

    public virtual Providers? Provider { get; set; }
}

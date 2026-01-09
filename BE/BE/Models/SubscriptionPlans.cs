using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class SubscriptionPlans
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public decimal? PriceMonthly { get; set; }

    public int? MaxProducts { get; set; }

    public decimal? CommissionRate { get; set; }

    public int? PriorityLevel { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<ProviderSubscriptions> ProviderSubscriptions { get; set; } = new List<ProviderSubscriptions>();
}

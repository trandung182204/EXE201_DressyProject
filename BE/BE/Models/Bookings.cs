using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class Bookings
{
    public long Id { get; set; }

    public long? CustomerId { get; set; }

    public decimal? TotalPrice { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    // Delivery info from checkout form
    public string? RecipientName { get; set; }

    public string? RecipientPhone { get; set; }

    public string? RecipientAddress { get; set; }

    public virtual ICollection<BookingItems> BookingItems { get; set; } = new List<BookingItems>();

    public virtual Users? Customer { get; set; }

    public virtual ICollection<Payments> Payments { get; set; } = new List<Payments>();

    public virtual ICollection<ProviderFeedbacks> ProviderFeedbacks { get; set; } = new List<ProviderFeedbacks>();
}

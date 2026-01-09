using System;
using System.Collections.Generic;

namespace BE.Models;

public partial class Payments
{
    public long Id { get; set; }

    public long? BookingId { get; set; }

    public string? PaymentMethod { get; set; }

    public decimal? Amount { get; set; }

    public string? Status { get; set; }

    public DateTime? PaidAt { get; set; }

    public virtual Bookings? Booking { get; set; }
}

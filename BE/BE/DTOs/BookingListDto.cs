
using System;
using System.Collections.Generic;

namespace BE.DTOs;

public class BookingListDto
{
    public long BookingId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string BookingStatus { get; set; } = string.Empty;
}

using System;
using System.Collections.Generic;

namespace BE.DTOs
{
    public class BookingDetailDto
    {
        public long BookingId { get; set; }
        public string CustomerName { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public string BookingStatus { get; set; } = "";
        public string PaymentStatus { get; set; } = "";
        public decimal TotalPrice { get; set; }

        public List<BookingItemDto> Items { get; set; } = new();
    }

    public class BookingItemDto
    {
        public string ProductName { get; set; } = "";
        public string VariantName { get; set; } = "";
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string ImageUrl { get; set; } = "";
    }
}
namespace BE.DTOs
{
    public class CustomerBookingListDto
    {
        public long BookingId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string BookingStatus { get; set; } = "";
        public string PaymentStatus { get; set; } = "";
        public string PaymentMethod { get; set; } = "";
        public decimal TotalPrice { get; set; }

        // Ship info (nếu có)
        public string? RecipientName { get; set; }
        public string? RecipientPhone { get; set; }
        public string? RecipientAddress { get; set; }
    }
}
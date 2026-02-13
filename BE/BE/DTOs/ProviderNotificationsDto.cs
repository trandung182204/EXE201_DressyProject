namespace BE.DTOs
{
    public class ProviderNotificationStateDto
    {
        public DateTimeOffset? LastSeenAt { get; set; }
    }

    public class ProviderOrderNotificationDto
    {
        public long BookingId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CustomerName { get; set; } = "Unknown";
        public decimal TotalPrice { get; set; }
        public string PaymentStatus { get; set; } = "UNPAID";
        public string BookingStatus { get; set; } = "PENDING";
    }

    public class MarkSeenRequest
    {
        public DateTimeOffset? LastSeenAt { get; set; }
    }
}
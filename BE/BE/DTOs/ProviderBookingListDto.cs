namespace BE.DTOs
{
    public class ProviderBookingListDto
    {
        public long BookingId { get; set; }
        public string CustomerName { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public string BookingStatus { get; set; } = "";
        public string PaymentStatus { get; set; } = "";

        public int TotalItems { get; set; }
        public decimal ProviderRevenue { get; set; }
        public decimal ProviderCommission { get; set; }
    }
}

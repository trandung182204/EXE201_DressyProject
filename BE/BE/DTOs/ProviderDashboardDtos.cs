namespace BE.DTOs;

public class ProviderDashboardSummaryDto
{
    public decimal TotalRevenue { get; set; }   // NET
    public int TotalOrders { get; set; }        // booking count (COMPLETED)
    public int OrdersToday { get; set; }        // booking count (COMPLETED, today)
    public decimal RevenueToday { get; set; }   // NET (today)
}

public class ProviderRevenueByMonthDto
{
    public int Month { get; set; }              // 1..12
    public decimal Revenue { get; set; }        // NET
    public int Orders { get; set; }             // completed bookings
}
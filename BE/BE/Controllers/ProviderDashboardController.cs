using System.Security.Claims;
using BE.Data;
using BE.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BE.Controllers;

[ApiController]
[Route("api/provider/dashboard")]
[Authorize(Roles = "provider")]
public class ProviderDashboardController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public ProviderDashboardController(ApplicationDbContext db) => _db = db;

    private async Task<long> ResolveProviderIdAsync()
    {
        var pidStr = User.FindFirstValue("providerId");
        if (!string.IsNullOrWhiteSpace(pidStr) && long.TryParse(pidStr, out var pid))
            return pid;

        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(userIdStr) || !long.TryParse(userIdStr, out var userId))
            throw new UnauthorizedAccessException("Token thiếu userId/sub.");

        pid = await _db.Providers
            .Where(p => p.UserId == userId)
            .Select(p => p.Id)
            .FirstOrDefaultAsync();

        if (pid <= 0) throw new UnauthorizedAccessException("Provider not found.");
        return pid;
    }

    // Summary: total (range) + today
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var providerId = await ResolveProviderIdAsync();

        // VN timezone (Windows: "SE Asia Standard Time", Linux: "Asia/Ho_Chi_Minh")
        TimeZoneInfo vnTz;
        try { vnTz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); }
        catch { vnTz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh"); }

        // "Bây giờ" theo VN
        var nowVn = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTz);

        // ====== Range filter theo VN ======
        // from/to (query string) thường Kind=Unspecified -> coi như ngày VN
        DateTime startVn = from.HasValue
            ? from.Value.Date
            : new DateTime(nowVn.Year, nowVn.Month, 1);

        DateTime endExclusiveVn = to.HasValue
            ? to.Value.Date.AddDays(1)
            : nowVn.Date.AddDays(1);

        // đổi mốc VN -> UTC để query DB (timestamptz)
        var startUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(startVn, DateTimeKind.Unspecified), vnTz);
        var endExclusiveUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(endExclusiveVn, DateTimeKind.Unspecified), vnTz);

        // ✅ chỉ lấy booking COMPLETED trong khoảng VN
        var baseQuery =
            from bi in _db.BookingItems.AsNoTracking()
            join b in _db.Bookings.AsNoTracking() on bi.BookingId equals b.Id
            where bi.ProviderId == providerId
                  && b.Status == "COMPLETED"
                  && b.CreatedAt >= startUtc && b.CreatedAt < endExclusiveUtc
            select new { bi, b };

        var totalRevenue = await baseQuery
            .SumAsync(x => (decimal?)((x.bi.Price ?? 0) - (x.bi.CommissionAmount ?? 0))) ?? 0;

        var totalOrders = await baseQuery
            .Select(x => x.b.Id)
            .Distinct()
            .CountAsync();

        // ====== Today theo VN ======
        var todayStartVn = nowVn.Date;              // 00:00 hôm nay theo VN
        var todayEndVn = todayStartVn.AddDays(1);   // 00:00 ngày mai theo VN

        var todayStartUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(todayStartVn, DateTimeKind.Unspecified), vnTz);
        var todayEndUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(todayEndVn, DateTimeKind.Unspecified), vnTz);

        var todayQuery =
            from bi in _db.BookingItems.AsNoTracking()
            join b in _db.Bookings.AsNoTracking() on bi.BookingId equals b.Id
            where bi.ProviderId == providerId
                  && b.Status == "COMPLETED"
                  && b.CreatedAt >= todayStartUtc && b.CreatedAt < todayEndUtc
            select new { bi, b };

        var revenueToday = await todayQuery
            .SumAsync(x => (decimal?)((x.bi.Price ?? 0) - (x.bi.CommissionAmount ?? 0))) ?? 0;

        var ordersToday = await todayQuery
            .Select(x => x.b.Id)
            .Distinct()
            .CountAsync();

        return Ok(new
        {
            success = true,
            data = new ProviderDashboardSummaryDto
            {
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                OrdersToday = ordersToday,
                RevenueToday = revenueToday
            }
        });
    }

    // Chart: revenue by month
    [HttpGet("revenue-by-month")]
    public async Task<IActionResult> RevenueByMonth([FromQuery] int? year)
    {
        var providerId = await ResolveProviderIdAsync();
        var y = year ?? DateTime.UtcNow.Year;

        var start = new DateTime(y, 1, 1);
        var end = start.AddYears(1);

        var q =
            from bi in _db.BookingItems.AsNoTracking()
            join b in _db.Bookings.AsNoTracking() on bi.BookingId equals b.Id
            where bi.ProviderId == providerId
                  && b.Status == "COMPLETED"
                  && b.CreatedAt >= start && b.CreatedAt < end
            select new { bi, b };

        var grouped = await q
            .GroupBy(x => x.b.CreatedAt!.Value.Month)
            .Select(g => new ProviderRevenueByMonthDto
            {
                Month = g.Key,
                Revenue = g.Sum(z => (decimal?)((z.bi.Price ?? 0) - (z.bi.CommissionAmount ?? 0))) ?? 0,
                Orders = g.Select(z => z.b.Id).Distinct().Count()
            })
            .ToListAsync();

        // fill 12 tháng
        var result = Enumerable.Range(1, 12)
            .Select(m => grouped.FirstOrDefault(x => x.Month == m)
                        ?? new ProviderRevenueByMonthDto { Month = m, Revenue = 0, Orders = 0 })
            .ToList();

        return Ok(new { success = true, data = result });
    }
    
}
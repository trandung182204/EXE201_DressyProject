using System.Security.Claims;
using BE.Data;
using BE.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BE.Controllers
{
    [ApiController]
    [Route("api/provider/notifications")]
    [Authorize(Roles = "provider")]
    public class ProviderNotificationsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public ProviderNotificationsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /api/provider/notifications/state
        [HttpGet("state")]
        public async Task<IActionResult> GetState()
        {
            var providerId = await ResolveProviderIdAsync();

            var lastSeen = await _db.Providers
                .Where(p => p.Id == providerId)
                .Select(p => p.LastNotificationSeenAt) // ⚠️ bạn cần map property trong Model Providers (bên dưới mình chỉ)
                .FirstOrDefaultAsync();

            return Ok(new
            {
                success = true,
                data = new ProviderNotificationStateDto
                {
                    LastSeenAt = lastSeen
                }
            });
        }

        // GET: /api/provider/notifications/unread-count
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var providerId = await ResolveProviderIdAsync();

            var lastSeen = await _db.Providers
                .Where(p => p.Id == providerId)
                .Select(p => p.LastNotificationSeenAt)
                .FirstOrDefaultAsync();

            var q = _db.BookingItems
                .Where(bi => bi.ProviderId == providerId)
                .Select(bi => bi.BookingId)
                .Distinct()
                .Join(_db.Bookings,
                      bookingId => bookingId,
                      b => b.Id,
                      (bookingId, b) => b);

            if (lastSeen.HasValue)
                q = q.Where(b => b.CreatedAt > lastSeen.Value.UtcDateTime);

            var count = await q.CountAsync();

            return Ok(new { success = true, data = count });
        }

        // GET: /api/provider/notifications/orders?limit=5
        [HttpGet("orders")]
        public async Task<IActionResult> GetRecentOrders([FromQuery] int limit = 5)
        {
            var providerId = await ResolveProviderIdAsync();
            limit = Math.Clamp(limit, 1, 20);

            // lastSeen để FE có thể highlight NEW (optional)
            var lastSeen = await _db.Providers
                .Where(p => p.Id == providerId)
                .Select(p => p.LastNotificationSeenAt)
                .FirstOrDefaultAsync();

            // Lấy booking thuộc provider (distinct booking) + join user + payment mới nhất
            // EF khó "DISTINCT ON", nên mình làm theo cách an toàn: lấy bookingIds trước rồi query bookings
            var bookingIds = await _db.BookingItems
                .Where(bi => bi.ProviderId == providerId)
                .Select(bi => bi.BookingId)
                .Distinct()
                .ToListAsync();

            var bookings = await _db.Bookings
                .Where(b => bookingIds.Contains(b.Id))
                .OrderByDescending(b => b.CreatedAt)
                .Take(limit)
                .Select(b => new
                {
                    b.Id,
                    b.CreatedAt,
                    b.TotalPrice,
                    b.Status,
                    CustomerName = _db.Users.Where(u => u.Id == b.CustomerId).Select(u => u.FullName).FirstOrDefault(),
                    PaymentStatus = _db.Payments
                        .Where(p => p.BookingId == b.Id)
                        .OrderByDescending(p => p.PaidAt)    // paid_at DESC NULLS LAST gần giống
                        .ThenByDescending(p => p.Id)
                        .Select(p => p.Status)
                        .FirstOrDefault()
                })
                .ToListAsync();

            var result = bookings.Select(x => new ProviderOrderNotificationDto
            {
                BookingId = x.Id,
                CreatedAt = x.CreatedAt ?? DateTime.UtcNow,
                TotalPrice = x.TotalPrice ?? 0,
                BookingStatus = x.Status ?? "PENDING",
                CustomerName = string.IsNullOrWhiteSpace(x.CustomerName) ? "Unknown" : x.CustomerName!,
                PaymentStatus = string.IsNullOrWhiteSpace(x.PaymentStatus) ? "UNPAID" : x.PaymentStatus!
            }).ToList();

            return Ok(new
            {
                success = true,
                data = new
                {
                    lastSeenAt = lastSeen,
                    items = result
                }
            });
        }

        // PUT: /api/provider/notifications/seen
        [HttpPut("seen")]
        public async Task<IActionResult> MarkSeen([FromBody] MarkSeenRequest? req)
        {
            var providerId = await ResolveProviderIdAsync();

            var provider = await _db.Providers.FirstOrDefaultAsync(p => p.Id == providerId);
            if (provider == null)
                return NotFound(new { success = false, message = "Provider not found" });

            var t = req?.LastSeenAt ?? DateTimeOffset.UtcNow;
            provider.LastNotificationSeenAt = t;

            provider.UpdatedAt = DateTime.UtcNow; // nếu bạn có cột UpdatedAt
            await _db.SaveChangesAsync();

            return Ok(new { success = true, data = new ProviderNotificationStateDto { LastSeenAt = t } });
        }

        private async Task<long> ResolveProviderIdAsync()
        {
            var providerIdStr = User.FindFirstValue("providerId");
            if (!string.IsNullOrWhiteSpace(providerIdStr) && long.TryParse(providerIdStr, out var providerId))
                return providerId;

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrWhiteSpace(userIdStr) || !long.TryParse(userIdStr, out var userId))
                throw new UnauthorizedAccessException("Token thiếu userId/sub.");

            providerId = await _db.Providers
                .Where(p => p.UserId == userId)
                .Select(p => p.Id)
                .FirstOrDefaultAsync();

            if (providerId <= 0)
                throw new UnauthorizedAccessException("User chưa có Providers record (providerId không tồn tại).");

            return providerId;
        }
    }
}
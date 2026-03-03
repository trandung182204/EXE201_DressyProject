using Microsoft.AspNetCore.Mvc;
using BE.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatisticsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public StatisticsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("monthly")]
        public async Task<IActionResult> GetMonthlyStatistics()
        {
            var now = DateTime.Now;
            var currentMonth = now.Month;
            var currentYear = now.Year;

            // Profit: tổng tiền từ Bookings trong tháng
            var profit = await _context.Bookings
                .Where(b => b.CreatedAt.HasValue && b.CreatedAt.Value.Month == currentMonth && b.CreatedAt.Value.Year == currentYear)
                .SumAsync(b => b.TotalPrice ?? 0);

            // Refunds: tổng tiền hoàn từ Payments có Status = 'Refunded' trong tháng
            var refunds = await _context.Payments
                .Where(p => p.PaidAt.HasValue && p.PaidAt.Value.Month == currentMonth && p.PaidAt.Value.Year == currentYear && p.Status == "Refunded")
                .SumAsync(p => p.Amount ?? 0);

            // Expenses: chưa có bảng Expenses, trả về 0
            decimal expenses = 0;

            return Ok(new
            {
                success = true,
                data = new
                {
                    profit,
                    refunds,
                    expenses
                },
                message = "Monthly statistics fetched successfully"
            });
        }
    }
}

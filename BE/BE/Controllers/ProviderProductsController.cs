using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BE.Data;
using BE.Services.Interfaces;

namespace BE.Controllers
{
    [ApiController]
    [Route("api/provider/products")]
    [Authorize(Roles = "provider")]
    public class ProviderProductsController : ControllerBase
    {
        private readonly IProductsService _productsService;
        private readonly ApplicationDbContext _db;

        public ProviderProductsController(IProductsService productsService, ApplicationDbContext db)
        {
            _productsService = productsService;
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyProducts()
        {
            // 1) Ưu tiên lấy providerId từ claim
            var providerIdStr = User.FindFirstValue("providerId");
            long providerId;

            if (!string.IsNullOrWhiteSpace(providerIdStr) && long.TryParse(providerIdStr, out providerId))
            {
                var data = await _productsService.GetProductsByProviderAsync(providerId);
                return Ok(new { success = true, data, message = "Fetched successfully" });
            }

            // 2) Nếu token thiếu providerId -> fallback lấy userId rồi query providers
            // NameIdentifier thường được map từ "sub"
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrWhiteSpace(userIdStr) || !long.TryParse(userIdStr, out var userId))
            {
                return Unauthorized(new { success = false, message = "Token thiếu userId/sub." });
            }

            providerId = await _db.Providers
                .Where(p => p.UserId == userId)
                .Select(p => p.Id)
                .FirstOrDefaultAsync();

            if (providerId <= 0)
            {
                return Unauthorized(new { success = false, message = "User chưa có Providers record (providerId không tồn tại)." });
            }

            var data2 = await _productsService.GetProductsByProviderAsync(providerId);
            return Ok(new { success = true, data = data2, message = "Fetched successfully" });
        }
    }
}

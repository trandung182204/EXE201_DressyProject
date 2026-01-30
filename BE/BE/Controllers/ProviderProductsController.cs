using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BE.Data;
using BE.Services.Interfaces;
using BE.DTOs;

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
        [HttpPost]
        public async Task<IActionResult> CreateMyProduct([FromBody] CreateProviderProductDto dto)
        {
            if (dto == null)
                return BadRequest(new { success = false, message = "Body is required" });

            long providerId;
            try
            {
                providerId = await ResolveProviderIdAsync();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }

            try
            {
                var product = await _productsService.AddForProviderAsync(providerId, dto);

                // load lại full info để trả về
                var created = await _db.Products
                    .Include(p => p.Category)
                    .Include(p => p.ProductImages)
                    .Include(p => p.ProductVariants)
                    .FirstOrDefaultAsync(p => p.Id == product.Id);

                return Ok(new { success = true, data = new { id = product.Id }, message = "Created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
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

        [HttpGet("{productId:long}")]
        public async Task<IActionResult> GetMyProductDetail(long productId)
        {
            // 1) Ưu tiên lấy providerId từ claim
            var providerIdStr = User.FindFirstValue("providerId");
            long providerId;

            if (!string.IsNullOrWhiteSpace(providerIdStr) && long.TryParse(providerIdStr, out providerId))
            {
                var detail = await _productsService.GetProductDetailByProviderAsync(providerId, productId);
                if (detail == null)
                    return NotFound(new { success = false, message = "Không tìm thấy sản phẩm hoặc không thuộc provider này." });

                return Ok(new { success = true, data = detail, message = "Fetched successfully" });
            }

            // 2) Fallback: lấy userId/sub -> query Providers lấy providerId
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

            var detail2 = await _productsService.GetProductDetailByProviderAsync(providerId, productId);
            if (detail2 == null)
                return NotFound(new { success = false, message = "Không tìm thấy sản phẩm hoặc không thuộc provider này." });

            return Ok(new { success = true, data = detail2, message = "Fetched successfully" });
        }
        [HttpDelete("{productId:long}")]
        public async Task<IActionResult> DeleteMyProduct(long productId)
        {
            long providerId;
            try
            {
                providerId = await ResolveProviderIdAsync();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }

            try
            {
                var ok = await _productsService.DeleteByProviderAsync(providerId, productId);
                if (!ok)
                    return NotFound(new { success = false, message = "Không tìm thấy sản phẩm hoặc không thuộc provider này." });

                return Ok(new { success = true, message = "Deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }


    }
}

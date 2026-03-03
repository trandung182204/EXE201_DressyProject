using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BE.Data;
using BE.DTOs;

namespace BE.Controllers
{
    [ApiController]
    [Route("api/provider")]
    [Authorize(Roles = "provider")]
    public class ProviderProfileController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public ProviderProfileController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            long providerId;
            try { providerId = await ResolveProviderIdAsync(); }
            catch (UnauthorizedAccessException ex)
            { return Unauthorized(new { success = false, message = ex.Message }); }

            var provider = await _db.Providers
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == providerId);

            if (provider == null)
                return NotFound(new { success = false, message = "Provider not found" });

            // ✅ OPTIONAL: trả thêm url runtime để FE render ảnh
            var logoUrl = provider.LogoFileId != null
                ? $"/api/media-files/{provider.LogoFileId}"
                : null;

            return Ok(new
            {
                success = true,
                data = provider,
                logoUrl,
                message = "Fetched successfully"
            });
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateMe([FromBody] UpdateProviderProfileDto dto)
        {
            if (dto == null)
                return BadRequest(new { success = false, message = "Body is required" });

            long providerId;
            try { providerId = await ResolveProviderIdAsync(); }
            catch (UnauthorizedAccessException ex)
            { return Unauthorized(new { success = false, message = ex.Message }); }

            var provider = await _db.Providers.FirstOrDefaultAsync(p => p.Id == providerId);
            if (provider == null)
                return NotFound(new { success = false, message = "Provider not found" });

            // Validate nhẹ (tuỳ bạn)
            if (!string.IsNullOrWhiteSpace(dto.ProviderType))
            {
                var pt = dto.ProviderType.Trim().ToUpper();
                if (pt != "BOTH" && pt != "SERVICE" && pt != "OUTFIT")
                    return BadRequest(new { success = false, message = "providerType must be BOTH | SERVICE | OUTFIT" });

                provider.ProviderType = pt;
            }

            if (!string.IsNullOrWhiteSpace(dto.BrandName))
                provider.BrandName = dto.BrandName.Trim();

            // Cho phép clear description bằng cách gửi "" (tuỳ bạn)
            if (dto.Description != null)
                provider.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();

            // ✅ CHANGED: LogoUrl -> LogoFileId (cho phép clear = null)
            // Quy ước:
            // - dto.LogoFileId = null => clear logo
            // - dto.LogoFileId > 0 => set logo
            // - dto.LogoFileId = 0 hoặc âm => ignore (tránh set rác)
            if (dto.LogoFileId.HasValue)
            {
                if (dto.LogoFileId.Value > 0)
                    provider.LogoFileId = dto.LogoFileId.Value;
                else
                    provider.LogoFileId = null; // nếu muốn "0" nghĩa là clear
            }

            provider.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            var logoUrl = provider.LogoFileId != null
                ? $"/api/media-files/{provider.LogoFileId}"
                : null;

            return Ok(new
            {
                success = true,
                data = provider,
                logoUrl,
                message = "Updated successfully"
            });
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

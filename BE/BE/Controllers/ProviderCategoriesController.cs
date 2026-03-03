using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BE.Data;
using BE.DTOs;
using BE.Models;

namespace BE.Controllers
{
    [ApiController]
    [Route("api/provider/categories")]
    [Authorize(Roles = "provider")]
    public class ProviderCategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public ProviderCategoriesController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyCategories([FromQuery] bool includeGlobal = true)
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

            var q = _db.Categories.AsNoTracking().AsQueryable();

            // ✅ Category của provider + (tuỳ chọn) category global (provider_id IS NULL)
            if (includeGlobal)
                q = q.Where(c => c.ProviderId == providerId || c.ProviderId == null);
            else
                q = q.Where(c => c.ProviderId == providerId);

            // Optional: chỉ lấy ACTIVE (nếu bạn muốn)
            // q = q.Where(c => c.Status == "ACTIVE");

            var data = await q
                .OrderBy(c => c.Name)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    ParentId = c.ParentId,
                    Description = c.Description,
                    Status = c.Status
                })
                .ToListAsync();

            return Ok(new { success = true, data, message = "Fetched successfully" });
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
        // DTO tạo category
        public class CreateCategoryDto
        {
            public string? Name { get; set; }
            public long? ParentId { get; set; }
            public string? Description { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> CreateMyCategory([FromBody] CreateCategoryDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { success = false, message = "Name is required" });

            long providerId;
            try
            {
                providerId = await ResolveProviderIdAsync();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }

            // optional: validate parentId tồn tại
            if (dto.ParentId != null)
            {
                var parentOk = await _db.Categories.AnyAsync(c => c.Id == dto.ParentId);
                if (!parentOk) return BadRequest(new { success = false, message = "Parent category not found" });
            }

            var cat = new Categories
            {
                Name = dto.Name.Trim(),
                ParentId = dto.ParentId,
                Description = dto.Description,
                Status = "ACTIVE",
                ProviderId = providerId
            };

            _db.Categories.Add(cat);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                data = new { id = cat.Id, name = cat.Name, parentId = cat.ParentId, description = cat.Description, status = cat.Status },
                message = "Created successfully"
            });
        }

    }
}

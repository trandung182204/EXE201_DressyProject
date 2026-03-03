using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BE.Services.Interfaces;
using BE.Models;
using BE.DTOs;
using System.Security.Claims;

namespace BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartsController : ControllerBase
    {
        private readonly ICartsService _service;
        public CartsController(ICartsService service)
        {
            _service = service;
        }

        // --- Basic CRUD (keep existing) ---

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAllAsync();
            return Ok(new { success = true, data, message = "Fetched successfully" });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound(new { success = false, data = (object?)null, message = "Not found" });
            return Ok(new { success = true, data = item, message = "Fetched successfully" });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Carts model)
        {
            var created = await _service.AddAsync(model);
            return Ok(new { success = true, data = created, message = "Created successfully" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Carts model)
        {
            var updated = await _service.UpdateAsync(id, model);
            if (updated == null) return NotFound(new { success = false, data = (object?)null, message = "Not found" });
            return Ok(new { success = true, data = updated, message = "Updated successfully" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted) return NotFound(new { success = false, data = (object?)null, message = "Not found" });
            return Ok(new { success = true, data = (object?)null, message = "Deleted successfully" });
        }

        // --- Per-user cart endpoints ---

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyCart()
        {
            if (!TryGetUserId(out var customerId))
                return Unauthorized(new { success = false, message = "Missing userId in token." });

            var detail = await _service.GetCartDetailAsync(customerId);
            return Ok(new { success = true, data = detail, message = "Fetched successfully" });
        }

        [Authorize]
        [HttpPost("me/items")]
        public async Task<IActionResult> AddToMyCart([FromBody] AddToCartDto dto)
        {
            if (!TryGetUserId(out var customerId))
                return Unauthorized(new { success = false, message = "Missing userId in token." });

            try
            {
                var item = await _service.AddToCartAsync(customerId, dto);
                return Ok(new { success = true, data = item, message = "Added to cart" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpDelete("me/items/{cartItemId}")]
        public async Task<IActionResult> RemoveFromMyCart(long cartItemId)
        {
            if (!TryGetUserId(out var customerId))
                return Unauthorized(new { success = false, message = "Missing userId in token." });

            var removed = await _service.RemoveCartItemAsync(customerId, cartItemId);
            if (!removed)
                return NotFound(new { success = false, message = "Cart item not found" });

            return Ok(new { success = true, message = "Removed from cart" });
        }

        [Authorize]
        [HttpDelete("me")]
        public async Task<IActionResult> ClearMyCart()
        {
            if (!TryGetUserId(out var customerId))
                return Unauthorized(new { success = false, message = "Missing userId in token." });

            await _service.ClearCartAsync(customerId);
            return Ok(new { success = true, message = "Cart cleared" });
        }

        private bool TryGetUserId(out long userId)
        {
            userId = 0;
            var v = User.FindFirst("userId")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;
            return !string.IsNullOrWhiteSpace(v) && long.TryParse(v, out userId);
        }
    }
}

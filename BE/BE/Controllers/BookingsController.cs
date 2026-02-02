using Microsoft.AspNetCore.Mvc;
using BE.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using BE.Models;

namespace BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingsService _service;
        public BookingsController(IBookingsService service)
        {
            _service = service;
        }

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
        public async Task<IActionResult> Create([FromBody] Bookings model)
        {
            var created = await _service.AddAsync(model);
            return Ok(new { success = true, data = created, message = "Created successfully" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Bookings model)
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

        [HttpGet("list")]
        public async Task<IActionResult> GetBookingList()
        {
            var data = await _service.GetBookingListAsync();
            return Ok(new
            {
                success = true,
                data,
                message = "Fetched successfully"
            });
        }
        [Authorize(Roles = "provider")]
        [HttpGet("provider")]
        public async Task<IActionResult> GetBookingsForProvider()
        {
            var providerIdStr = User.FindFirst("providerId")?.Value;

            if (string.IsNullOrWhiteSpace(providerIdStr) || !long.TryParse(providerIdStr, out var providerId))
                return Unauthorized(new { success = false, message = "Missing providerId in token." });

            var data = await _service.GetBookingListByProviderAsync(providerId);

            return Ok(new { success = true, data, message = "Fetched successfully" });
        }

    }
}

using Microsoft.AspNetCore.Mvc;
using BE.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using BE.Models;
using BE.DTOs;
using System.Threading.Tasks;
using System;

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
        public async Task<IActionResult> Get(long id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound(new { success = false, message = "Not found" });
            return Ok(new { success = true, data = item, message = "Fetched successfully" });
        }

        // API Chi tiết theo DTO bạn yêu cầu
        [HttpGet("{id}/detail")]
        public async Task<IActionResult> GetDetail(long id)
        {
            var item = await _service.GetBookingDetailAsync(id);
            if (item == null) return NotFound(new { success = false, message = "Booking not found" });
            return Ok(new { success = true, data = item, message = "Fetched successfully" });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Bookings model)
        {
            var created = await _service.AddAsync(model);
            return Ok(new { success = true, data = created, message = "Created successfully" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, [FromBody] Bookings model)
        {
            var updated = await _service.UpdateAsync(id, model);
            if (updated == null) return NotFound(new { success = false, message = "Not found" });
            return Ok(new { success = true, data = updated, message = "Updated successfully" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted) return NotFound(new { success = false, message = "Not found" });
            return Ok(new { success = true, message = "Deleted successfully" });
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetBookingList()
        {
            var data = await _service.GetBookingListAsync();
            return Ok(new { success = true, data, message = "Fetched successfully" });
        }



        [Authorize(Roles = "provider,PROVIDER")]
        [HttpGet("provider")]
        public async Task<IActionResult> GetBookingsForProvider(
            [FromQuery] DateOnly? from,
            [FromQuery] DateOnly? to,
            [FromQuery] string? status
        )
        {
            var providerIdStr = User.FindFirst("providerId")?.Value;

            if (string.IsNullOrWhiteSpace(providerIdStr) || !long.TryParse(providerIdStr, out var providerId))
                return Unauthorized(new { success = false, message = "Missing providerId in token." });

            var data = await _service.GetBookingListByProviderAsync(providerId, from, to, status);

            return Ok(new { success = true, data, message = "Fetched successfully" });
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(long id, [FromBody] UpdateBookingStatusDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Status))
                return BadRequest(new { success = false, message = "Status is required" });

            var ok = await _service.UpdateStatusAsync(id, dto.Status);
            if (!ok)
                return NotFound(new { success = false, message = "Booking not found" });

            return Ok(new { success = true, message = "Status updated successfully" });
        }
        [Authorize(Roles = "provider")]
        [HttpGet("provider/{id}/detail")]
        public async Task<IActionResult> GetDetailForProvider(long id)
        {
            var providerIdStr = User.FindFirst("providerId")?.Value;

            if (string.IsNullOrWhiteSpace(providerIdStr) || !long.TryParse(providerIdStr, out var providerId))
                return Unauthorized(new { success = false, message = "Missing providerId in token." });

            var item = await _service.GetBookingDetailForProviderAsync(id, providerId);

            if (item == null)
                return NotFound(new { success = false, message = "Booking not found (or not belong to provider)." });

            return Ok(new { success = true, data = item, message = "Fetched successfully" });
        }
    }
}
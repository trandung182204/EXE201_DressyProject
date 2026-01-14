using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BE.Services.Interfaces;
using BE.Models;
using BE.DTOs;

namespace BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUsersService _service;
        public UsersController(IUsersService service)
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
        public async Task<IActionResult> Create([FromBody] Users model)
        {
            var created = await _service.AddAsync(model);
            return Ok(new { success = true, data = created, message = "Created successfully" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Users model)
        {
            var updated = await _service.UpdateAsync(id, model);
            if (updated == null) return NotFound(new { success = false, data = (object?)null, message = "Not found" });
            return Ok(new { success = true, data = updated, message = "Updated successfully" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted) return NotFound(new { success = false, data = (object?)null, message = "Not found" });
            return Ok(new { success = true, data = (object?)null, message = "Deleted successfully" });
        }


        [HttpPut("{id}/status")]
public async Task<IActionResult> UpdateStatus(long id, [FromBody] UpdateUserStatusDto dto)
{
    if (string.IsNullOrWhiteSpace(dto.Status))
        return BadRequest(new { success = false, message = "Status is required" });

    var user = await _service.GetByIdAsync(id);
    if (user == null)
        return NotFound(new { success = false, message = "User not found" });

    user.Status = dto.Status;
    var updated = await _service.UpdateAsync(id, user);

    return Ok(new
    {
        success = true,
        data = updated,
        message = "Status updated successfully"
    });
}
}
}

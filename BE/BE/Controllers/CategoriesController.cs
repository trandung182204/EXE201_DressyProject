using Microsoft.AspNetCore.Mvc;
using BE.Services.Interfaces;
using BE.Models;

namespace BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoriesService _service;
        public CategoriesController(ICategoriesService service)
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
        public async Task<IActionResult> Create([FromBody] Categories model)
        {
            var created = await _service.AddAsync(model);
            return Ok(new { success = true, data = created, message = "Created successfully" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Categories model)
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
    }
}

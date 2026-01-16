using BE.Models;
using BE.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BE.Controllers
{
    [ApiController]
    [Route("api/provider-branches")]
    public class ProviderBranchesController : ControllerBase
    {
        private readonly IProviderBranchesService _service;

        public ProviderBranchesController(IProviderBranchesService service)
        {
            _service = service;
        }

        [HttpGet("provider/{providerId}")]
        public async Task<IActionResult> GetByProvider(int providerId)
        {
            var data = await _service.GetByProviderAsync(providerId);
            return Ok(new { success = true, data });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProviderBranches model)
        {
            var created = await _service.AddAsync(model);
            return Ok(new { success = true, data = created });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProviderBranches model)
        {
            var updated = await _service.UpdateAsync(id, model);
            if (updated == null)
                return NotFound(new { success = false, message = "Not found" });

            return Ok(new { success = true, data = updated });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _service.DeleteAsync(id);
            if (!result)
                return NotFound(new { success = false, message = "Not found" });

            return Ok(new { success = true, message = "Deleted" });
        }
    }
}

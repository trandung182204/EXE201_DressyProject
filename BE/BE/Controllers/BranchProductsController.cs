using BE.Models;
using BE.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using BE.DTOs;

namespace BE.Controllers
{
    [ApiController]
    [Route("api/branches/{branchId}/products")]
    public class BranchProductsController : ControllerBase
    {
        private readonly IProductsService _service;

        public BranchProductsController(IProductsService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Get(long branchId)
        {
            var data = await _service.GetByBranchAsync(branchId);
            return Ok(new { success = true, data });
        }

        [HttpPost]
        public async Task<IActionResult> Create(
        long branchId,
        [FromBody] CreateProductDto dto)
        {
            var product = await _service.AddToBranchAsync(branchId, dto);

            return Ok(new
            {
                success = true,
                data = product
            });
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            long branchId,
            long id,
            [FromBody] UpdateProductDto dto
        )
        {
            var updated = await _service.UpdateInBranchAsync(branchId, id, dto);

            if (updated == null)
                return NotFound(new
                {
                    success = false,
                    message = "Product not found in this branch"
                });

            return Ok(new { success = true, data = updated });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long branchId, long id)
        {
            var ok = await _service.DeleteInBranchAsync(branchId, id);
            if (!ok)
                return NotFound(new { success = false });

            return Ok(new { success = true });
        }
    }
}

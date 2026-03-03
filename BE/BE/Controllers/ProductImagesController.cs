using Microsoft.AspNetCore.Mvc;
using BE.Data;
using Microsoft.EntityFrameworkCore;

namespace BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductImagesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public ProductImagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.ProductImages
                .Select(x => new
                {
                    x.Id,
                    x.ProductId,
                    x.ImageFileId
                })
                .ToListAsync();

            return Ok(new { success = true, data });
        }
    }
}

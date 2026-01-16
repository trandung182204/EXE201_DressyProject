using Microsoft.AspNetCore.Mvc;
using BE.Models;
using BE.Data;
using System.Linq;

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
        public IActionResult GetAll()
        {
            var data = _context.ProductImages.ToList();
            return Ok(new { success = true, data });
        }
    }
}

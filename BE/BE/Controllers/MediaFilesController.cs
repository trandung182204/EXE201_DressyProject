using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BE.Data;
using BE.Models;

namespace BE.Controllers
{
    [ApiController]
    [Route("api/media-files")]
    public class MediaFilesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MediaFilesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("upload")]
        [RequestSizeLimit(20_000_000)] // 20MB (tuỳ bạn)
        public async Task<IActionResult> Upload([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { success = false, message = "File is required" });

            await using var ms = new MemoryStream();
            await file.CopyToAsync(ms);

            var entity = new MediaFiles
            {
                FileName = file.FileName,
                MimeType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType,
                FileSize = file.Length,
                Data = ms.ToArray(),
                CreatedAt = DateTime.UtcNow
            };

            _context.MediaFiles.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, data = new { fileId = entity.Id } });
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetFile(long id)
        {
            var f = await _context.MediaFiles.FirstOrDefaultAsync(x => x.Id == id);
            if (f == null) return NotFound(new { success = false, message = "File not found" });

            return File(f.Data, f.MimeType, f.FileName);
        }
    }
}

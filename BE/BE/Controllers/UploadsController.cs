using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/uploads")]
[ApiController]
public class UploadsController : ControllerBase
{
    [Authorize]
    [HttpPost("images")]
    [RequestSizeLimit(50 * 1024 * 1024)]
    public async Task<IActionResult> UploadImages([FromForm] List<IFormFile> files)
    {
        if (files == null || files.Count == 0)
            return BadRequest(new { success = false, message = "No files" });

        var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

        var urls = new List<string>();

        foreach (var file in files)
        {
            if (file.Length <= 0) continue;
            if (!file.ContentType.StartsWith("image/")) continue;

            var ext = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(uploadDir, fileName);

            using (var stream = System.IO.File.Create(fullPath))
            {
                await file.CopyToAsync(stream);
            }

            // trả url public
            var url = $"{Request.Scheme}://{Request.Host}/uploads/{fileName}";
            urls.Add(url);
        }

        return Ok(new { success = true, urls });
    }
}

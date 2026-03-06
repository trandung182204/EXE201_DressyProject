using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

[ApiController]
[Route("api/tryon")]
public class TryOnController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;

    private const string API_KEY = "b3e93676d2924689ac00120b594ec2b544c2c4b0c65d7f5bb9984003bef0608e";

    public TryOnController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost]
    public async Task<IActionResult> TryOn(IFormFile cloth_image, IFormFile model_image)
    {
        if (cloth_image == null || model_image == null)
            return BadRequest("Missing images");

        var client = _httpClientFactory.CreateClient();

        var form = new MultipartFormDataContent();

        form.Add(new StreamContent(cloth_image.OpenReadStream())
        {
            Headers = { ContentType = new MediaTypeHeaderValue(cloth_image.ContentType) }
        }, "cloth_image", cloth_image.FileName);

        form.Add(new StreamContent(model_image.OpenReadStream())
        {
            Headers = { ContentType = new MediaTypeHeaderValue(model_image.ContentType) }
        }, "model_image", model_image.FileName);

        form.Add(new StringContent("full_set"), "cloth_type");

        client.DefaultRequestHeaders.Add("X-API-KEY", API_KEY);

        var createTaskResponse = await client.PostAsync(
            "https://platform.fitroom.app/api/tryon/v2/tasks",
            form
        );

        var createJson = await createTaskResponse.Content.ReadAsStringAsync();
        var createData = JsonDocument.Parse(createJson);

        var taskId = createData.RootElement.GetProperty("task_id").GetString();

        string resultUrl = null;

        while (true)
        {
            await Task.Delay(3000);

            var statusResponse = await client.GetAsync(
                $"https://platform.fitroom.app/api/tryon/v2/tasks/{taskId}"
            );

            var statusJson = await statusResponse.Content.ReadAsStringAsync();
            var statusData = JsonDocument.Parse(statusJson);

            var status = statusData.RootElement.GetProperty("status").GetString();

            if (status == "COMPLETED")
            {
                resultUrl = statusData.RootElement.GetProperty("download_signed_url").GetString();
                break;
            }

            if (status == "FAILED")
            {
                return StatusCode(500, "AI processing failed");
            }
        }

        return Ok(new
        {
            success = true,
            image = resultUrl
        });
    }
}
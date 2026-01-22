using BE.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BE.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsCustomerController : ControllerBase
{
    private readonly IProductsCustomerService _service;

    public ProductsCustomerController(IProductsCustomerService service)
    {
        _service = service;
    }

    // GET /api/products?status=active
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status = null)
    {
        var data = await _service.GetAllAsync(status);
        return Ok(data);
    }
}

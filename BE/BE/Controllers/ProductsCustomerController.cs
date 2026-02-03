// ProductsCustomerController.cs
using BE.DTOs;
using BE.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BE.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsCustomerController : ControllerBase
{
    private readonly IProductsCustomerService _service;
    public ProductsCustomerController(IProductsCustomerService service) => _service = service;

    // GET /api/ProductsCustomer/listing?status=active&page=1&pageSize=9&sortBy=createdAt&sortDir=desc&minPrice=10&maxPrice=100&categoryIds=1&categoryIds=2&sizes=S&colors=Red
    [HttpGet("listing")]
    public async Task<IActionResult> Listing([FromQuery] ProductListQuery q)
    {
        var data = await _service.GetListingAsync(q);
        return Ok(data);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDetail(long id)
    {
        var data = await _service.GetProductDetailAsync(id);
        if (data == null) return NotFound();
        return Ok(data);
    }
}

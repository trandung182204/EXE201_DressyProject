using BE.DTOs;
using BE.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BE.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductReviewsController : ControllerBase
{
    private readonly IProductReviewsService _service;

    public ProductReviewsController(IProductReviewsService service)
    {
        _service = service;
    }

    // GET /api/ProductReviews
    [HttpGet]
    public async Task<IActionResult> GetReviews([FromQuery] ProductReviewQueryDto query)
    {
        try
        {
            var data = await _service.GetReviewsAsync(query);
            return Ok(data);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET /api/ProductReviews/eligibility?productId=123
    [HttpGet("eligibility")]
    [Authorize]
    public async Task<IActionResult> CheckEligibility([FromQuery] long productId)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var customerId))
            {
                return Unauthorized(new { message = "Token không hợp lệ." });
            }

            var result = await _service.CheckEligibilityAsync(customerId, productId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // POST /api/ProductReviews
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateReview([FromForm] CreateProductReviewDto request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var customerId))
            {
                return Unauthorized(new { message = "Token không hợp lệ." });
            }

            var review = await _service.CreateReviewAsync(customerId, request);
            return Ok(review);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi tạo đánh giá.", details = ex.Message });
        }
    }
}

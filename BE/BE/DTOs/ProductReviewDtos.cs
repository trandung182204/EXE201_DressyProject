using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace BE.DTOs;

public class ProductReviewDto
{
    public long Id { get; set; }
    public long? ProductId { get; set; }
    public long? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public long? BookingItemId { get; set; }
    public int? Rating { get; set; }
    public string? Comment { get; set; }
    public string? Status { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string[]? ImageUrls { get; set; }
}

public class CreateProductReviewDto
{
    public long ProductId { get; set; }
    // BookingItemId is now resolved server-side via eligibility check
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public List<IFormFile>? Images { get; set; }
}

public class ProductReviewQueryDto
{
    public long? ProductId { get; set; }
    public long? BookingItemId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class ReviewEligibilityDto
{
    public bool CanReview { get; set; }
    public long? BookingItemId { get; set; }
    public string? Message { get; set; }
}


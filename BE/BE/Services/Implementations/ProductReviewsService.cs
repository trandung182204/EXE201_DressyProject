using BE.DTOs;
using BE.Models;
using BE.Repositories.Interfaces;
using BE.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace BE.Services.Implementations;

public class ProductReviewsService : IProductReviewsService
{
    private readonly IProductReviewsRepository _reviewsRepo;
    private readonly IWebHostEnvironment _env;

    public ProductReviewsService(IProductReviewsRepository reviewsRepo, IWebHostEnvironment env)
    {
        _reviewsRepo = reviewsRepo;
        _env = env;
    }

    public async Task<PagedResult<ProductReviewDto>> GetReviewsAsync(ProductReviewQueryDto query)
    {
        return await _reviewsRepo.GetReviewsAsync(query);
    }

    /// <summary>
    /// Check if a customer is eligible to review a specific product.
    /// Returns the bookingItemId if eligible.
    /// </summary>
    public async Task<ReviewEligibilityDto> CheckEligibilityAsync(long customerId, long productId)
    {
        var eligibleItem = await _reviewsRepo.GetEligibleBookingItemAsync(customerId, productId);

        if (eligibleItem == null)
        {
            return new ReviewEligibilityDto
            {
                CanReview = false,
                BookingItemId = null,
                Message = "Bạn chỉ có thể đánh giá sản phẩm sau khi đã mua và chưa đánh giá."
            };
        }

        return new ReviewEligibilityDto
        {
            CanReview = true,
            BookingItemId = eligibleItem.Id,
            Message = null
        };
    }

    public async Task<ProductReviewDto> CreateReviewAsync(long customerId, CreateProductReviewDto request)
    {
        // 1. Server-side eligibility: find the eligible booking item for this customer + product
        var eligibleItem = await _reviewsRepo.GetEligibleBookingItemAsync(customerId, request.ProductId);
        if (eligibleItem == null)
        {
            throw new InvalidOperationException("Bạn không đủ điều kiện để đánh giá sản phẩm này. Bạn cần mua sản phẩm trước hoặc đã đánh giá rồi.");
        }

        long bookingItemId = eligibleItem.Id;

        // 2. Double-check: verify no existing review for this booking item (race condition guard)
        var existingReview = await _reviewsRepo.GetReviewByBookingItemAsync(bookingItemId);
        if (existingReview != null)
        {
            throw new InvalidOperationException("Bạn đã đánh giá cho đơn hàng này rồi.");
        }

        // 3. Security: verify the booking item belongs to this customer and matches the product
        var bookingItem = await _reviewsRepo.GetBookingItemWithDetailsAsync(bookingItemId);
        if (bookingItem == null ||
            bookingItem.Booking == null ||
            bookingItem.Booking.CustomerId != customerId)
        {
            throw new InvalidOperationException("Đơn hàng không thuộc về bạn.");
        }

        if (bookingItem.ProductId != request.ProductId)
        {
            throw new InvalidOperationException("Sản phẩm không khớp với đơn hàng.");
        }

        // 4. Handle image uploads
        var imageUrls = new List<string>();
        if (request.Images != null && request.Images.Any())
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "reviews");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            foreach (var file in request.Images)
            {
                if (file.Length > 0)
                {
                    var fileExtension = Path.GetExtension(file.FileName);
                    var newFileName = $"{Guid.NewGuid()}{fileExtension}";
                    var filePath = Path.Combine(uploadsFolder, newFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var fileUrl = $"/uploads/reviews/{newFileName}";
                    imageUrls.Add(fileUrl);
                }
            }
        }

        // 5. Create the review entity
        var newReview = new ProductReviews
        {
            ProductId = request.ProductId,
            CustomerId = customerId,
            BookingItemId = bookingItemId,
            Rating = request.Rating,
            Comment = request.Comment,
            ImageUrls = imageUrls.ToArray(),
            Status = "PUBLISHED",
            CreatedAt = DateTime.UtcNow
        };

        var savedReview = await _reviewsRepo.CreateReviewAsync(newReview);

        // 6. Return the DTO
        return new ProductReviewDto
        {
            Id = savedReview.Id,
            ProductId = savedReview.ProductId,
            CustomerId = savedReview.CustomerId,
            BookingItemId = savedReview.BookingItemId,
            Rating = savedReview.Rating,
            Comment = savedReview.Comment,
            Status = savedReview.Status,
            CreatedAt = savedReview.CreatedAt,
            ImageUrls = savedReview.ImageUrls
        };
    }
}

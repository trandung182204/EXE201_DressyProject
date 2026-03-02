using BE.DTOs;
using BE.Models;

namespace BE.Repositories.Interfaces;

public interface IProductReviewsRepository
{
    Task<PagedResult<ProductReviewDto>> GetReviewsAsync(ProductReviewQueryDto query);
    Task<ProductReviews> CreateReviewAsync(ProductReviews review);
    Task<ProductReviews?> GetReviewByBookingItemAsync(long bookingItemId);
    /// <summary>
    /// Find an eligible BookingItem for a customer to review a product.
    /// Returns the first BookingItem where the customer purchased the product and has not already reviewed it.
    /// </summary>
    Task<BookingItems?> GetEligibleBookingItemAsync(long customerId, long productId);
    /// <summary>
    /// Get a BookingItem with its parent Booking loaded (for security validation).
    /// </summary>
    Task<BookingItems?> GetBookingItemWithDetailsAsync(long bookingItemId);
    /// <summary>
    /// Check if a customer has already reviewed a specific product (any booking item).
    /// </summary>
    Task<bool> HasReviewedProductAsync(long customerId, long productId);
}


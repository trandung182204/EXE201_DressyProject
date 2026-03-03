using BE.Data;
using BE.DTOs;
using BE.Models;
using BE.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BE.Repositories.Implementations;

public class ProductReviewsRepository : IProductReviewsRepository
{
    private readonly ApplicationDbContext _db;

    public ProductReviewsRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<ProductReviewDto>> GetReviewsAsync(ProductReviewQueryDto query)
    {
        var q = _db.ProductReviews
            .Include(r => r.Customer)
            .AsNoTracking()
            .AsQueryable();

        if (query.ProductId.HasValue)
        {
            q = q.Where(r => r.ProductId == query.ProductId.Value);
        }

        if (query.BookingItemId.HasValue)
        {
            q = q.Where(r => r.BookingItemId == query.BookingItemId.Value);
        }

        var totalItems = await q.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)query.PageSize);

        var items = await q
            .OrderByDescending(r => r.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(r => new ProductReviewDto
            {
                Id = r.Id,
                ProductId = r.ProductId,
                CustomerId = r.CustomerId,
                CustomerName = r.Customer != null ? r.Customer.FullName : null,
                BookingItemId = r.BookingItemId,
                Rating = r.Rating,
                Comment = r.Comment,
                Status = r.Status,
                CreatedAt = r.CreatedAt,
                ImageUrls = r.ImageUrls
            })
            .ToListAsync();

        return new PagedResult<ProductReviewDto>
        {
            Page = query.Page,
            PageSize = query.PageSize,
            TotalItems = totalItems,
            TotalPages = totalPages,
            Items = items
        };
    }

    public async Task<ProductReviews?> GetReviewByBookingItemAsync(long bookingItemId)
    {
        return await _db.ProductReviews
            .FirstOrDefaultAsync(r => r.BookingItemId == bookingItemId);
    }

    public async Task<ProductReviews> CreateReviewAsync(ProductReviews review)
    {
        _db.ProductReviews.Add(review);
        await _db.SaveChangesAsync();
        return review;
    }

    /// <summary>
    /// Find the first BookingItem where:
    /// - The booking belongs to the specified customer
    /// - The item matches the specified product
    /// - The customer has NOT already reviewed this product (any booking item)
    /// </summary>
    public async Task<BookingItems?> GetEligibleBookingItemAsync(long customerId, long productId)
    {
        // If customer already reviewed this product, not eligible
        var alreadyReviewed = await HasReviewedProductAsync(customerId, productId);
        if (alreadyReviewed) return null;

        return await _db.BookingItems
            .Include(bi => bi.Booking)
            .Where(bi =>
                bi.ProductId == productId &&
                bi.Booking != null &&
                bi.Booking.CustomerId == customerId
            )
            .OrderByDescending(bi => bi.Booking!.CreatedAt)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Check if a customer has already reviewed a specific product (any booking item).
    /// </summary>
    public async Task<bool> HasReviewedProductAsync(long customerId, long productId)
    {
        return await _db.ProductReviews
            .AnyAsync(r => r.CustomerId == customerId && r.ProductId == productId);
    }

    /// <summary>
    /// Get a BookingItem with its parent Booking loaded for security validation.
    /// </summary>
    public async Task<BookingItems?> GetBookingItemWithDetailsAsync(long bookingItemId)
    {
        return await _db.BookingItems
            .Include(bi => bi.Booking)
            .FirstOrDefaultAsync(bi => bi.Id == bookingItemId);
    }
}


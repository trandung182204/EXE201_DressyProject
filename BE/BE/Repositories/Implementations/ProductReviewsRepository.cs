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
    /// - The booking status is valid (COMPLETED, PAID, CONFIRMED)
    /// - No review has been submitted for this booking item yet
    /// </summary>
    public async Task<BookingItems?> GetEligibleBookingItemAsync(long customerId, long productId)
    {
        var validStatuses = new[] { "COMPLETED", "PAID", "CONFIRMED" };

        return await _db.BookingItems
            .Include(bi => bi.Booking)
            .Where(bi =>
                bi.ProductId == productId &&
                bi.Booking != null &&
                bi.Booking.CustomerId == customerId &&
                validStatuses.Contains(bi.Booking.Status!) &&
                !_db.ProductReviews.Any(r => r.BookingItemId == bi.Id)
            )
            .OrderByDescending(bi => bi.Booking!.CreatedAt)
            .FirstOrDefaultAsync();
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


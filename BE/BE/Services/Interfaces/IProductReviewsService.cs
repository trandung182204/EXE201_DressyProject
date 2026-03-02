using BE.DTOs;

namespace BE.Services.Interfaces;

public interface IProductReviewsService
{
    Task<PagedResult<ProductReviewDto>> GetReviewsAsync(ProductReviewQueryDto query);
    Task<ProductReviewDto> CreateReviewAsync(long customerId, CreateProductReviewDto request);
    Task<ReviewEligibilityDto> CheckEligibilityAsync(long customerId, long productId);
}


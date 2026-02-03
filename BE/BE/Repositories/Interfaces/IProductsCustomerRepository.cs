using BE.DTOs;
using BE.Models;

namespace BE.Repositories.Interfaces;

public interface IProductsCustomerRepository
{
    Task<PagedResult<ProductListCustomerItemDto>> GetListingAsync(ProductListQuery q);
    Task<ProductDetailDto?> GetProductDetailAsync(long id);
}
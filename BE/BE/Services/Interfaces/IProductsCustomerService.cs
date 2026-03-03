using BE.DTOs;
using BE.Models;

namespace BE.Services.Interfaces;

public interface IProductsCustomerService
{
     Task<PagedResult<ProductListCustomerItemDto>> GetListingAsync(ProductListQuery q);
     Task<ProductDetailDto?> GetProductDetailAsync(long id);
}


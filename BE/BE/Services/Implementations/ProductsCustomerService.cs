// ProductsCustomerService.cs
using BE.DTOs;
using BE.Repositories.Interfaces;
using BE.Services.Interfaces;

namespace BE.Services.Implementations;

public class ProductsCustomerService : IProductsCustomerService
{
    private readonly IProductsCustomerRepository _repo;
    public ProductsCustomerService(IProductsCustomerRepository repo) => _repo = repo;

    public Task<PagedResult<ProductListCustomerItemDto>> GetListingAsync(ProductListQuery q)
        => _repo.GetListingAsync(q);

    public Task<ProductDetailDto?> GetProductDetailAsync(long id)
        => _repo.GetProductDetailAsync(id);
}

using BE.DTOs;
using System.Linq;
using BE.Repositories.Interfaces;
using BE.Services.Interfaces;
using BE.Models;

namespace BE.Services.Implementations;

public class ProductsCustomerService : IProductsCustomerService
{
    private readonly IProductsCustomerRepository _repo;

    public ProductsCustomerService(IProductsCustomerRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<ProductListItemDto>> GetAllAsync(string? status = null)
    {
        var products = await _repo.GetAllAsync(status);

        return products.Select(p => new ProductListItemDto
        {
            Id = p.Id,
            Name = p.Name,
            CategoryName = p.Category?.Name,
            ThumbnailUrl = p.ProductImages?.Select(i => i.ImageUrl).FirstOrDefault(),
            MinPricePerDay = p.ProductVariants?.Min(v => (decimal?)v.PricePerDay),
            Status = p.Status
        }).ToList();
    }
}
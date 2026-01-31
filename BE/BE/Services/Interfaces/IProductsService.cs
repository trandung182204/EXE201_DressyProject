using System.Collections.Generic;
using System.Threading.Tasks;
using BE.Models;
using BE.DTOs;

namespace BE.Services.Interfaces
{
    public interface IProductsService
    {
        Task<IEnumerable<Products>> GetAllAsync();
        Task<Products?> GetByIdAsync(long id);
        Task<Products> AddAsync(Products model);
        Task<Products?> UpdateAsync(long id, Products model);
        Task<bool> DeleteAsync(long id);
        Task<bool> UpdateStatusAsync(long id, string status);
        Task<IEnumerable<ProductListItemDto>> GetProductsByProviderAsync(long providerId);
        Task<Products> AddForProviderAsync(long providerId, CreateProviderProductDto dto);
        Task<ProductDetailDto?> GetProductDetailByProviderAsync(long providerId, long productId);
        Task<bool> DeleteByProviderAsync(long providerId, long productId);

    }
}

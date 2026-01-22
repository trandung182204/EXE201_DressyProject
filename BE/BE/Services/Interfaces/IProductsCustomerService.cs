using BE.DTOs;
using BE.Models;

namespace BE.Services.Interfaces;

public interface IProductsCustomerService
{
     Task<List<ProductListItemDto>> GetAllAsync(string? status = null);
}


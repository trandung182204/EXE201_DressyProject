using BE.Models;

namespace BE.Repositories.Interfaces;

public interface IProductsCustomerRepository
{
    Task<List<Products>> GetAllAsync(string? status = null);
}
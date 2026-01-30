using System.Collections.Generic;
using System.Threading.Tasks;
using BE.Models;

namespace BE.Repositories.Interfaces
{
    public interface IProductsRepository
    {
        Task<IEnumerable<Products>> GetAllAsync();
        Task<Products?> GetByIdAsync(int id);
        Task<Products> AddAsync(Products model);
        Task<Products?> UpdateAsync(int id, Products model);
        Task<bool> DeleteAsync(int id);

        List<Products> GetByProviderId(long providerId);

    }
}

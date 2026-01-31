using System.Collections.Generic;
using System.Threading.Tasks;
using BE.Models;

namespace BE.Repositories.Interfaces
{
    public interface IProductsRepository
    {
        Task<IEnumerable<Products>> GetAllAsync();
        Task<Products?> GetByIdAsync(long id);
        Task<Products> AddAsync(Products model);
        Task<Products?> UpdateAsync(long id, Products model);
        Task<bool> DeleteAsync(long id);

        List<Products> GetByProviderId(long providerId);

    }
}

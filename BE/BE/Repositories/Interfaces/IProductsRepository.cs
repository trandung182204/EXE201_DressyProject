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

        Task<IEnumerable<Products>> GetByBranchAsync(long branchId);
        Task<Products?> GetByBranchAndIdAsync(long branchId, long productId);

        List<Products> GetByProviderId(long providerId);

    }
}

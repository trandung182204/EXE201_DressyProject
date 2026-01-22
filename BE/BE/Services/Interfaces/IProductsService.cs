using System.Collections.Generic;
using System.Threading.Tasks;
using BE.Models;
using BE.DTOs;

namespace BE.Services.Interfaces
{
    public interface IProductsService
    {
        Task<IEnumerable<Products>> GetAllAsync();
        Task<Products?> GetByIdAsync(int id);
        Task<Products> AddAsync(Products model);
        Task<Products?> UpdateAsync(int id, Products model);
        Task<bool> DeleteAsync(int id);
        Task<bool> UpdateStatusAsync(long id, string status);
        Task<IEnumerable<Products>> GetByBranchAsync(long branchId);
        Task<Products?> GetByBranchAndIdAsync(long branchId, long productId);
        Task<Products> AddToBranchAsync(long branchId, CreateProductDto dto);
        Task<Products?> UpdateInBranchAsync(long branchId, long productId, UpdateProductDto dto);
        Task<bool> DeleteInBranchAsync(long branchId, long productId);

    }
}

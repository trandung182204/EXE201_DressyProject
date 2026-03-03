using System.Collections.Generic;
using System.Threading.Tasks;
using BE.Models;

namespace BE.Repositories.Interfaces
{
    public interface ICategoriesRepository
    {
        Task<IEnumerable<Categories>> GetAllAsync();
        Task<Categories?> GetByIdAsync(int id);
        Task<Categories> AddAsync(Categories model);
        Task<Categories?> UpdateAsync(int id, Categories model);
        Task<bool> DeleteAsync(int id);
    }
}

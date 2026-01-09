using System.Collections.Generic;
using System.Threading.Tasks;
using BE.Models;

namespace BE.Services.Interfaces
{
    public interface ICartsService
    {
        Task<IEnumerable<Carts>> GetAllAsync();
        Task<Carts?> GetByIdAsync(int id);
        Task<Carts> AddAsync(Carts model);
        Task<Carts?> UpdateAsync(int id, Carts model);
        Task<bool> DeleteAsync(int id);
    }
}

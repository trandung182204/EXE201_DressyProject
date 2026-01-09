using System.Collections.Generic;
using System.Threading.Tasks;
using BE.Models;

namespace BE.Repositories.Interfaces
{
    public interface ICartItemsRepository
    {
        Task<IEnumerable<CartItems>> GetAllAsync();
        Task<CartItems?> GetByIdAsync(int id);
        Task<CartItems> AddAsync(CartItems model);
        Task<CartItems?> UpdateAsync(int id, CartItems model);
        Task<bool> DeleteAsync(int id);
    }
}

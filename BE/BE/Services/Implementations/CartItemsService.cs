using System.Collections.Generic;
using System.Threading.Tasks;
using BE.Models;
using BE.Services.Interfaces;
using BE.Repositories.Interfaces;

namespace BE.Services.Implementations
{
    public class CartItemsService : ICartItemsService
    {
        private readonly ICartItemsRepository _repo;
        public CartItemsService(ICartItemsRepository repo)
        {
            _repo = repo;
        }
        public async Task<IEnumerable<CartItems>> GetAllAsync() => await _repo.GetAllAsync();
        public async Task<CartItems?> GetByIdAsync(int id) => await _repo.GetByIdAsync(id);
        public async Task<CartItems> AddAsync(CartItems model) => await _repo.AddAsync(model);
        public async Task<CartItems?> UpdateAsync(int id, CartItems model) => await _repo.UpdateAsync(id, model);
        public async Task<bool> DeleteAsync(int id) => await _repo.DeleteAsync(id);
    }
}

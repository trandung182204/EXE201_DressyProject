using System.Collections.Generic;
using System.Threading.Tasks;
using BE.Models;
using BE.Services.Interfaces;
using BE.Repositories.Interfaces;

namespace BE.Services.Implementations
{
    public class CartsService : ICartsService
    {
        private readonly ICartsRepository _repo;
        public CartsService(ICartsRepository repo)
        {
            _repo = repo;
        }
        public async Task<IEnumerable<Carts>> GetAllAsync() => await _repo.GetAllAsync();
        public async Task<Carts?> GetByIdAsync(int id) => await _repo.GetByIdAsync(id);
        public async Task<Carts> AddAsync(Carts model) => await _repo.AddAsync(model);
        public async Task<Carts?> UpdateAsync(int id, Carts model) => await _repo.UpdateAsync(id, model);
        public async Task<bool> DeleteAsync(int id) => await _repo.DeleteAsync(id);
    }
}

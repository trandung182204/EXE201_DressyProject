using System.Collections.Generic;
using System.Threading.Tasks;
using BE.Models;
using BE.Services.Interfaces;
using BE.Repositories.Interfaces;

namespace BE.Services.Implementations
{
    public class CategoriesService : ICategoriesService
    {
        private readonly ICategoriesRepository _repo;
        public CategoriesService(ICategoriesRepository repo)
        {
            _repo = repo;
        }
        public async Task<IEnumerable<Categories>> GetAllAsync() => await _repo.GetAllAsync();
        public async Task<Categories?> GetByIdAsync(int id) => await _repo.GetByIdAsync(id);
        public async Task<Categories> AddAsync(Categories model) => await _repo.AddAsync(model);
        public async Task<Categories?> UpdateAsync(int id, Categories model) => await _repo.UpdateAsync(id, model);
        public async Task<bool> DeleteAsync(int id) => await _repo.DeleteAsync(id);
    }
}

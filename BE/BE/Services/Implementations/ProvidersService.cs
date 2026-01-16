using BE.Models;
using BE.Repositories.Interfaces;
using BE.Services.Interfaces;

namespace BE.Services.Implementations
{
    public class ProvidersService : IProvidersService
    {
        private readonly IProvidersRepository _repo;
        public ProvidersService(IProvidersRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<Providers>> GetAllAsync()
        {
            return await _repo.GetAllAsync();
        }

        public async Task<Providers?> GetByIdAsync(int id)
        {
            return await _repo.GetByIdAsync(id);
        }

        public async Task<Providers> AddAsync(Providers model)
        {
            return await _repo.AddAsync(model);
        }

        public async Task<Providers?> UpdateAsync(int id, Providers model)
        {
            return await _repo.UpdateAsync(id, model);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repo.DeleteAsync(id);
        }
    }
}

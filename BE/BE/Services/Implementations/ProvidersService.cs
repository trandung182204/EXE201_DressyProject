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
            => await _repo.GetAllAsync();

        public async Task<Providers?> GetByIdAsync(long id)
            => await _repo.GetByIdAsync(id);

        public async Task<Providers> AddAsync(Providers model)
            => await _repo.AddAsync(model);

        public async Task<Providers?> UpdateAsync(long id, Providers model)
            => await _repo.UpdateAsync(id, model);

        public async Task<bool> DeleteAsync(long id)
            => await _repo.DeleteAsync(id);
    }
}

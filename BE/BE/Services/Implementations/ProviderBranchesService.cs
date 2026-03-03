using BE.Models;
using BE.Repositories.Interfaces;
using BE.Services.Interfaces;

namespace BE.Services.Implementations
{
    public class ProviderBranchesService : IProviderBranchesService
    {
        private readonly IProviderBranchesRepository _repo;

        public ProviderBranchesService(IProviderBranchesRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<ProviderBranches>> GetByProviderAsync(int providerId)
        {
            return await _repo.GetByProviderIdAsync(providerId);
        }

        public async Task<ProviderBranches?> GetByIdAsync(int id)
        {
            return await _repo.GetByIdAsync(id);
        }

        public async Task<ProviderBranches> AddAsync(ProviderBranches model)
        {
            return await _repo.AddAsync(model);
        }

        public async Task<ProviderBranches?> UpdateAsync(int id, ProviderBranches model)
        {
            return await _repo.UpdateAsync(id, model);
        }

        public async Task<bool> DeleteAsync(long id)
        {
            return await _repo.DeleteAsync(id);
        }
    }
}

using BE.Models;

namespace BE.Repositories.Interfaces
{
    public interface IProviderBranchesRepository
    {
        Task<IEnumerable<ProviderBranches>> GetByProviderIdAsync(int providerId);
        Task<ProviderBranches?> GetByIdAsync(int id);
        Task<ProviderBranches> AddAsync(ProviderBranches model);
        Task<ProviderBranches?> UpdateAsync(int id, ProviderBranches model);
        Task<bool> DeleteAsync(long id);
    }
}

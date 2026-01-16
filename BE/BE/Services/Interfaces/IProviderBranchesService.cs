using BE.Models;

namespace BE.Services.Interfaces
{
    public interface IProviderBranchesService
    {
        Task<IEnumerable<ProviderBranches>> GetByProviderAsync(int providerId);
        Task<ProviderBranches?> GetByIdAsync(int id);
        Task<ProviderBranches> AddAsync(ProviderBranches model);
        Task<ProviderBranches?> UpdateAsync(int id, ProviderBranches model);
        Task<bool> DeleteAsync(long id);
    }
}

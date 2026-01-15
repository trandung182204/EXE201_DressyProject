using BE.Models;

namespace BE.Repositories.Interfaces
{
    public interface IProvidersRepository
    {
        Task<IEnumerable<Providers>> GetAllAsync();
        Task<Providers?> GetByIdAsync(long id);
        Task<Providers> AddAsync(Providers model);
        Task<Providers?> UpdateAsync(long id, Providers model);
        Task<bool> DeleteAsync(long id);
    }
}

using BE.Models;

namespace BE.Services.Interfaces
{
    public interface IProvidersService
    {
        Task<IEnumerable<Providers>> GetAllAsync();
        Task<Providers?> GetByIdAsync(long id);
        Task<Providers> AddAsync(Providers model);
        Task<Providers?> UpdateAsync(long id, Providers model);
        Task<bool> DeleteAsync(long id);
    }
}

using BE.Models;

namespace BE.Services.Interfaces
{
    public interface IProvidersService
    {
        Task<IEnumerable<Providers>> GetAllAsync();
        Task<Providers?> GetByIdAsync(int id);
        Task<Providers> AddAsync(Providers model);
        Task<Providers?> UpdateAsync(int id, Providers model);
        Task<bool> DeleteAsync(int id);
    }
}

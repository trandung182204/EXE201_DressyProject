using Microsoft.EntityFrameworkCore;
using BE.Data;
using BE.Models;
using BE.Repositories.Interfaces;

namespace BE.Repositories.Implementations
{
    public class ProvidersRepository : IProvidersRepository
    {
        private readonly ApplicationDbContext _context;

        public ProvidersRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Providers>> GetAllAsync()
            => await _context.Providers.ToListAsync();

        public async Task<Providers?> GetByIdAsync(long id)
            => await _context.Providers.FindAsync(id);

        public async Task<Providers> AddAsync(Providers model)
        {
            model.CreatedAt = DateTime.Now;
            model.UpdatedAt = DateTime.Now;

            _context.Providers.Add(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<Providers?> UpdateAsync(long id, Providers model)
        {
            var item = await _context.Providers.FindAsync(id);
            if (item == null) return null;

            model.UpdatedAt = DateTime.Now;

            _context.Entry(item).CurrentValues.SetValues(model);
            return item;
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var item = await _context.Providers.FindAsync(id);
            if (item == null) return false;

            _context.Providers.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

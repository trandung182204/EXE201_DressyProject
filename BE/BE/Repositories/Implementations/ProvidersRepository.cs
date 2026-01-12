using BE.Models;
using BE.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using BE.Data;

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
        {
            return await _context.Providers.ToListAsync();
        }

        public async Task<Providers?> GetByIdAsync(int id)
        {
            return await _context.Providers.FindAsync(id);
        }

        public async Task<Providers> AddAsync(Providers model)
        {
            _context.Providers.Add(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<Providers?> UpdateAsync(int id, Providers model)
        {
            var existing = await _context.Providers.FindAsync(id);
            if (existing == null) return null;
            _context.Entry(existing).CurrentValues.SetValues(model);
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _context.Providers.FindAsync(id);
            if (existing == null) return false;
            _context.Providers.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

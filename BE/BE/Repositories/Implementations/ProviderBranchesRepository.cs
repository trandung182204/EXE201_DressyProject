using BE.Data;
using BE.Models;
using BE.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BE.Repositories.Implementations
{
    public class ProviderBranchesRepository : IProviderBranchesRepository
    {
        private readonly ApplicationDbContext _context;

        public ProviderBranchesRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProviderBranches>> GetByProviderIdAsync(int providerId)
        {
            return await _context.ProviderBranches
                .Where(x => x.ProviderId == providerId)
                .ToListAsync();
        }

        public async Task<ProviderBranches?> GetByIdAsync(int id)
        {
            return await _context.ProviderBranches.FindAsync(id);
        }

        public async Task<ProviderBranches> AddAsync(ProviderBranches model)
        {
            _context.ProviderBranches.Add(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<ProviderBranches?> UpdateAsync(int id, ProviderBranches model)
        {
            var existing = await _context.ProviderBranches.FindAsync(id);
            if (existing == null) return null;

            _context.Entry(existing).CurrentValues.SetValues(model);
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var existing = await _context.ProviderBranches.FindAsync(id);
            if (existing == null) return false;

            _context.ProviderBranches.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BE.Models;
using BE.Data;
using BE.Repositories.Interfaces;

namespace BE.Repositories.Implementations
{
    public class CategoriesRepository : ICategoriesRepository
    {
        private readonly ApplicationDbContext _context;
        public CategoriesRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Categories>> GetAllAsync() => await _context.Categories.ToListAsync();
        public async Task<Categories?> GetByIdAsync(int id) => await _context.Categories.FindAsync(id);
        public async Task<Categories> AddAsync(Categories model)
        {
            _context.Categories.Add(model);
            await _context.SaveChangesAsync();
            return model;
        }
        public async Task<Categories?> UpdateAsync(int id, Categories model)
        {
            var item = await _context.Categories.FindAsync(id);
            if (item == null) return null;
            _context.Entry(item).CurrentValues.SetValues(model);
            await _context.SaveChangesAsync();
            return item;
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var item = await _context.Categories.FindAsync(id);
            if (item == null) return false;
            _context.Categories.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

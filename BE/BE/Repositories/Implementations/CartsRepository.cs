using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BE.Models;
using BE.Data;
using BE.Repositories.Interfaces;

namespace BE.Repositories.Implementations
{
    public class CartsRepository : ICartsRepository
    {
        private readonly ApplicationDbContext _context;
        public CartsRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Carts>> GetAllAsync() => await _context.Carts.ToListAsync();
        public async Task<Carts?> GetByIdAsync(int id) => await _context.Carts.FindAsync(id);
        public async Task<Carts> AddAsync(Carts model)
        {
            _context.Carts.Add(model);
            await _context.SaveChangesAsync();
            return model;
        }
        public async Task<Carts?> UpdateAsync(int id, Carts model)
        {
            var item = await _context.Carts.FindAsync(id);
            if (item == null) return null;
            _context.Entry(item).CurrentValues.SetValues(model);
            await _context.SaveChangesAsync();
            return item;
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var item = await _context.Carts.FindAsync(id);
            if (item == null) return false;
            _context.Carts.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

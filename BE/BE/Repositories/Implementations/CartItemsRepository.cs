using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BE.Models;
using BE.Data;
using BE.Repositories.Interfaces;

namespace BE.Repositories.Implementations
{
    public class CartItemsRepository : ICartItemsRepository
    {
        private readonly ApplicationDbContext _context;
        public CartItemsRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<CartItems>> GetAllAsync() => await _context.CartItems.ToListAsync();
        public async Task<CartItems?> GetByIdAsync(int id) => await _context.CartItems.FindAsync(id);
        public async Task<CartItems> AddAsync(CartItems model)
        {
            _context.CartItems.Add(model);
            await _context.SaveChangesAsync();
            return model;
        }
        public async Task<CartItems?> UpdateAsync(int id, CartItems model)
        {
            var item = await _context.CartItems.FindAsync(id);
            if (item == null) return null;
            _context.Entry(item).CurrentValues.SetValues(model);
            await _context.SaveChangesAsync();
            return item;
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var item = await _context.CartItems.FindAsync(id);
            if (item == null) return false;
            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

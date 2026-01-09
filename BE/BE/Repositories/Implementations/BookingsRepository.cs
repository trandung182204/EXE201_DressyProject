using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BE.Models;
using BE.Data;
using BE.Repositories.Interfaces;

namespace BE.Repositories.Implementations
{
    public class BookingsRepository : IBookingsRepository
    {
        private readonly ApplicationDbContext _context;
        public BookingsRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Bookings>> GetAllAsync() => await _context.Bookings.ToListAsync();
        public async Task<Bookings?> GetByIdAsync(int id) => await _context.Bookings.FindAsync(id);
        public async Task<Bookings> AddAsync(Bookings model)
        {
            _context.Bookings.Add(model);
            await _context.SaveChangesAsync();
            return model;
        }
        public async Task<Bookings?> UpdateAsync(int id, Bookings model)
        {
            var item = await _context.Bookings.FindAsync(id);
            if (item == null) return null;
            _context.Entry(item).CurrentValues.SetValues(model);
            await _context.SaveChangesAsync();
            return item;
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var item = await _context.Bookings.FindAsync(id);
            if (item == null) return false;
            _context.Bookings.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

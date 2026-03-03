using BE.Data;
using BE.Models;
using BE.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BE.Repositories.Implementations
{
    public class PaymentsRepository : IPaymentsRepository
    {
        private readonly ApplicationDbContext _context;
        public PaymentsRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Payments>> GetAllAsync()
        {
            return await _context.Payments.ToListAsync();
        }

        public async Task<Payments?> GetByIdAsync(int id)
        {
            return await _context.Payments.FindAsync(id);
        }

        public async Task<Payments> AddAsync(Payments payment)
        {
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            return payment;
        }

        public async Task<Payments?> UpdateAsync(int id, Payments payment)
        {
            var existing = await _context.Payments.FindAsync(id);
            if (existing == null) return null;
            _context.Entry(existing).CurrentValues.SetValues(payment);
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _context.Payments.FindAsync(id);
            if (existing == null) return false;
            _context.Payments.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

using BE.Data;
using BE.Models;
using BE.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BE.Repositories.Implementations
{
    public class FeedbackResponsesRepository : IFeedbackResponsesRepository
    {
        private readonly ApplicationDbContext _context;
        public FeedbackResponsesRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<FeedbackResponses>> GetAllAsync()
        {
            return await _context.FeedbackResponses.ToListAsync();
        }

        public async Task<FeedbackResponses?> GetByIdAsync(int id)
        {
            return await _context.FeedbackResponses.FindAsync(id);
        }

        public async Task<FeedbackResponses> AddAsync(FeedbackResponses feedback)
        {
            _context.FeedbackResponses.Add(feedback);
            await _context.SaveChangesAsync();
            return feedback;
        }

        public async Task<FeedbackResponses?> UpdateAsync(int id, FeedbackResponses feedback)
        {
            var existing = await _context.FeedbackResponses.FindAsync(id);
            if (existing == null) return null;
            _context.Entry(existing).CurrentValues.SetValues(feedback);
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _context.FeedbackResponses.FindAsync(id);
            if (existing == null) return false;
            _context.FeedbackResponses.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

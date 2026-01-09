using System.Collections.Generic;
using System.Threading.Tasks;
using BE.Models;
using BE.Services.Interfaces;
using BE.Repositories.Interfaces;

namespace BE.Services.Implementations
{
    public class BookingsService : IBookingsService
    {
        private readonly IBookingsRepository _repo;
        public BookingsService(IBookingsRepository repo)
        {
            _repo = repo;
        }
        public async Task<IEnumerable<Bookings>> GetAllAsync() => await _repo.GetAllAsync();
        public async Task<Bookings?> GetByIdAsync(int id) => await _repo.GetByIdAsync(id);
        public async Task<Bookings> AddAsync(Bookings model) => await _repo.AddAsync(model);
        public async Task<Bookings?> UpdateAsync(int id, Bookings model) => await _repo.UpdateAsync(id, model);
        public async Task<bool> DeleteAsync(int id) => await _repo.DeleteAsync(id);
    }
}

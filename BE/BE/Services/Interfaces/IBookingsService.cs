using System.Collections.Generic;
using System.Threading.Tasks;
using BE.Models;
using BE.DTOs;

namespace BE.Services.Interfaces
{
    public interface IBookingsService
    {
        Task<IEnumerable<Bookings>> GetAllAsync();
        Task<Bookings?> GetByIdAsync(int id);
        Task<Bookings> AddAsync(Bookings model);
        Task<Bookings?> UpdateAsync(int id, Bookings model);
        Task<bool> DeleteAsync(int id);

        Task<List<BookingListDto>> GetBookingListAsync();
        Task<List<ProviderBookingListDto>> GetBookingListByProviderAsync(long providerId);

    }
}

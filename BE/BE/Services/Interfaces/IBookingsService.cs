using BE.DTOs;
using BE.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BE.Services.Interfaces
{
    public interface IBookingsService
    {
        Task<IEnumerable<Bookings>> GetAllAsync();
        Task<Bookings?> GetByIdAsync(long id); // Chú ý: sửa int thành long nếu ID trong DB là bigint
        Task<Bookings> AddAsync(Bookings model);
        Task<Bookings?> UpdateAsync(long id, Bookings model);
        Task<bool> DeleteAsync(long id);

        // --- CÁC HÀM CŨ CỦA BẠN ---
        Task<List<BookingListDto>> GetBookingListAsync();
        Task<List<ProviderBookingListDto>> GetBookingListByProviderAsync(long providerId);

        // --- CÁC HÀM CẦN THÊM MỚI ---
        Task<BookingDetailDto?> GetBookingDetailAsync(long id);
        Task<bool> UpdateStatusAsync(long id, string status);
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
using BE.Models;
using Microsoft.Data.SqlClient;
using BE.Services.Interfaces;
using BE.Repositories.Interfaces;
using BE.DTOs;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace BE.Services.Implementations
{
    public class BookingsService : IBookingsService
    {
        private readonly IBookingsRepository _repo;
        private readonly string _connectionString;

        public BookingsService(
            IBookingsRepository repo,
            IConfiguration configuration)
        {
            _repo = repo;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<Bookings>> GetAllAsync()
            => await _repo.GetAllAsync();

        public async Task<Bookings?> GetByIdAsync(int id)
            => await _repo.GetByIdAsync(id);

        public async Task<Bookings> AddAsync(Bookings model)
            => await _repo.AddAsync(model);

        public async Task<Bookings?> UpdateAsync(int id, Bookings model)
            => await _repo.UpdateAsync(id, model);

        public async Task<bool> DeleteAsync(int id)
            => await _repo.DeleteAsync(id);

        public async Task<List<BookingListDto>> GetBookingListAsync()
        {
            var list = new List<BookingListDto>();

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(@"
                SELECT
                    b.id AS BookingId,
                    u.full_name AS CustomerName,
                    b.created_at AS CreatedAt,
                    ISNULL(p.status, 'UNPAID') AS PaymentStatus,
                    b.total_price AS TotalPrice,
                    ISNULL(p.payment_method, '') AS PaymentMethod,
                    b.status AS BookingStatus
                FROM bookings b
                JOIN users u ON u.id = b.customer_id
                LEFT JOIN payments p ON p.booking_id = b.id
                ORDER BY b.created_at DESC
            ", conn);

            await conn.OpenAsync();
            using var rd = await cmd.ExecuteReaderAsync();

            while (await rd.ReadAsync())
            {
                list.Add(new BookingListDto
                {
                    BookingId = rd.GetInt64(0),
                    CustomerName = rd.GetString(1),
                    CreatedAt = rd.GetDateTime(2),
                    PaymentStatus = rd.GetString(3),
                    TotalPrice = rd.GetDecimal(4),
                    PaymentMethod = rd.GetString(5),
                    BookingStatus = rd.GetString(6)
                });
            }

            return list;
        }
    }
}

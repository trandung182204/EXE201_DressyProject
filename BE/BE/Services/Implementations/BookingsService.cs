using System.Collections.Generic;
using System.Threading.Tasks;
using BE.Models;
using BE.Services.Interfaces;
using BE.Repositories.Interfaces;
using BE.DTOs;
using Microsoft.Extensions.Configuration;
using System.Linq;
using Npgsql;

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
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
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

            await using var conn = new NpgsqlConnection(_connectionString);
            await using var cmd = new NpgsqlCommand(@"
                SELECT
                    b.id AS booking_id,
                    u.full_name AS customer_name,
                    b.created_at AS created_at,
                    COALESCE(p.status, 'UNPAID') AS payment_status,
                    b.total_price AS total_price,
                    COALESCE(p.payment_method, '') AS payment_method,
                    b.status AS booking_status
                FROM bookings b
                JOIN users u ON u.id = b.customer_id
                LEFT JOIN payments p ON p.booking_id = b.id
                ORDER BY b.created_at DESC
            ", conn);

            await conn.OpenAsync();
            await using var rd = await cmd.ExecuteReaderAsync();

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

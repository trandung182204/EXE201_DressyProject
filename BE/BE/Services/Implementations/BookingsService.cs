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
        public async Task<List<ProviderBookingListDto>> GetBookingListByProviderAsync(long providerId)
        {
            var list = new List<ProviderBookingListDto>();

            await using var conn = new NpgsqlConnection(_connectionString);
            await using var cmd = new NpgsqlCommand(@"
    SELECT
        b.id AS booking_id,
        u.full_name AS customer_name,
        b.created_at AS created_at,
        b.status AS booking_status,

        COALESCE(pay.status, 'UNPAID') AS payment_status,

        COUNT(bi.id) AS total_items,
        COALESCE(SUM(bi.price), 0) AS provider_revenue,
        COALESCE(SUM(bi.commission_amount), 0) AS provider_commission
    FROM bookings b
    JOIN users u ON u.id = b.customer_id
    JOIN booking_items bi ON bi.booking_id = b.id

    LEFT JOIN (
        SELECT DISTINCT ON (booking_id)
            booking_id,
            status
        FROM payments
        ORDER BY booking_id, paid_at DESC NULLS LAST, id DESC
    ) pay ON pay.booking_id = b.id

    WHERE bi.provider_id = @providerId
    GROUP BY
        b.id, u.full_name, b.created_at, b.status, pay.status
    ORDER BY b.created_at DESC
", conn);


            cmd.Parameters.AddWithValue("@providerId", providerId);

            await conn.OpenAsync();
            await using var rd = await cmd.ExecuteReaderAsync();

            while (await rd.ReadAsync())
            {
                list.Add(new ProviderBookingListDto
                {
                    BookingId = rd.GetInt64(0),
                    CustomerName = rd.GetString(1),
                    CreatedAt = rd.GetDateTime(2),
                    BookingStatus = rd.GetString(3),
                    PaymentStatus = rd.GetString(4),
                    TotalItems = rd.GetInt32(5),
                    ProviderRevenue = rd.GetDecimal(6),
                    ProviderCommission = rd.GetDecimal(7)
                });
            }

            return list;
        }

    }
}

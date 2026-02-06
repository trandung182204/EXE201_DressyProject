using System.Collections.Generic;
using System.Threading.Tasks;
using BE.Models;
using BE.Services.Interfaces;
using BE.Repositories.Interfaces;
using BE.DTOs;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;

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

        // --- Basic CRUD ---
        public async Task<IEnumerable<Bookings>> GetAllAsync() => await _repo.GetAllAsync();
        public async Task<Bookings?> GetByIdAsync(long id) => await _repo.GetByIdAsync((int)id);
        public async Task<Bookings> AddAsync(Bookings model) => await _repo.AddAsync(model);
        public async Task<Bookings?> UpdateAsync(long id, Bookings model) => await _repo.UpdateAsync((int)id, model);
        public async Task<bool> DeleteAsync(long id) => await _repo.DeleteAsync((int)id);

        // --- Custom Methods ---

        public async Task<bool> UpdateStatusAsync(long id, string status)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await using var cmd = new NpgsqlCommand("UPDATE bookings SET status = @status WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@status", status);
            cmd.Parameters.AddWithValue("@id", id);
            await conn.OpenAsync();
            var affected = await cmd.ExecuteNonQueryAsync();
            return affected > 0;
        }

        public async Task<List<BookingListDto>> GetBookingListAsync()
        {
            var list = new List<BookingListDto>();
            await using var conn = new NpgsqlConnection(_connectionString);
            await using var cmd = new NpgsqlCommand(@"
                SELECT
                    b.id,
                    u.full_name,
                    b.created_at,
                    COALESCE(p.status, 'UNPAID'),
                    b.total_price,
                    COALESCE(p.payment_method, ''),
                    b.status
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
                    CustomerName = rd.IsDBNull(1) ? "Unknown" : rd.GetString(1),
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
                    b.id,
                    u.full_name,
                    b.created_at,
                    b.status,
                    COALESCE(pay.status, 'UNPAID'),
                    COUNT(bi.id),
                    COALESCE(SUM(bi.price), 0),
                    COALESCE(SUM(bi.commission_amount), 0)
                FROM bookings b
                JOIN users u ON u.id = b.customer_id
                JOIN booking_items bi ON bi.booking_id = b.id
                LEFT JOIN (
                    SELECT DISTINCT ON (booking_id) booking_id, status
                    FROM payments
                    ORDER BY booking_id, paid_at DESC NULLS LAST, id DESC
                ) pay ON pay.booking_id = b.id
                WHERE bi.provider_id = @providerId
                GROUP BY b.id, u.full_name, b.created_at, b.status, pay.status
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
                    CustomerName = rd.IsDBNull(1) ? "Unknown" : rd.GetString(1),
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

        // --- CẬP NHẬT: LOGIC MAPPING CHÍNH XÁC THEO DTO BẠN GỬI ---
        public async Task<BookingDetailDto?> GetBookingDetailAsync(long id)
        {
            BookingDetailDto? detail = null;

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            // 1. Lấy thông tin Header (Booking Info)
            // Chỉ lấy các trường có trong BookingDetailDto của bạn
            await using (var cmd = new NpgsqlCommand(@"
                SELECT 
                    b.id, 
                    u.full_name, 
                    b.created_at, 
                    b.status, 
                    COALESCE(p.status, 'UNPAID') as payment_status,
                    b.total_price
                FROM bookings b
                LEFT JOIN users u ON b.customer_id = u.id
                LEFT JOIN payments p ON p.booking_id = b.id
                WHERE b.id = @id
                LIMIT 1
            ", conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                await using var rd = await cmd.ExecuteReaderAsync();
                if (await rd.ReadAsync())
                {
                    detail = new BookingDetailDto
                    {
                        BookingId = rd.GetInt64(0),
                        CustomerName = rd.IsDBNull(1) ? "Unknown" : rd.GetString(1),
                        CreatedAt = rd.GetDateTime(2),
                        BookingStatus = rd.GetString(3),
                        PaymentStatus = rd.GetString(4),
                        TotalPrice = rd.GetDecimal(5),
                        Items = new List<BookingItemDto>() // Khởi tạo list rỗng
                    };
                }
            }

            if (detail == null) return null;

            // 2. Lấy danh sách Items
            await using (var cmdItems = new NpgsqlCommand(@"
                SELECT 
                    p.name as product_name,
                    COALESCE(bi.variant_name, '') as variant_name,
                    bi.quantity,
                    bi.price,
                    bi.start_date,
                    bi.end_date,
                    COALESCE(bi.image_url, '') as image_url
                FROM booking_items bi
                LEFT JOIN products p ON bi.product_id = p.id
                WHERE bi.booking_id = @id
            ", conn))
            {
                cmdItems.Parameters.AddWithValue("@id", id);
                await using var rdItems = await cmdItems.ExecuteReaderAsync();
                while (await rdItems.ReadAsync())
                {
                    // Xử lý DateOnly từ DateTime của DB trả về
                    DateOnly? start = rdItems.IsDBNull(4) ? null : DateOnly.FromDateTime(rdItems.GetDateTime(4));
                    DateOnly? end = rdItems.IsDBNull(5) ? null : DateOnly.FromDateTime(rdItems.GetDateTime(5));

                    detail.Items.Add(new BookingItemDto
                    {
                        ProductName = rdItems.IsDBNull(0) ? "Unknown" : rdItems.GetString(0),
                        VariantName = rdItems.GetString(1),
                        Quantity = rdItems.GetInt32(2),
                        Price = rdItems.GetDecimal(3),
                        StartDate = start,
                        EndDate = end,
                        ImageUrl = rdItems.GetString(6)
                    });
                }
            }

            return detail;
        }
    }
}
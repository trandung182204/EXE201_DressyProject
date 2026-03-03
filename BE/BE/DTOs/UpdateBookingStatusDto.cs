using System;
using System.Collections.Generic;

namespace BE.DTOs
{
    // DTO nhận dữ liệu khi update status
    public class UpdateBookingStatusDto
    {
        public string Status { get; set; } = null!;
    }

    // DTO trả về cho Popup chi tiết

    public class BookingItemDetailDto
    {
        public long Id { get; set; }
        public string ServiceName { get; set; } = "Dịch vụ"; // Tên dịch vụ lấy từ bảng Services
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
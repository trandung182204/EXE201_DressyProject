namespace BE.DTOs
{
    public class UpdateProviderProfileDto
    {
        public string? BrandName { get; set; }

        // ✅ CHANGED: lưu file id thay vì url
        // - null  : clear logo
        // - > 0   : set logo mới
        // - không gửi field này : giữ nguyên logo
        public long? LogoFileId { get; set; }

        public string? Description { get; set; }

        // BOTH | SERVICE | OUTFIT
        public string? ProviderType { get; set; }

        // ACTIVE | SUSPENDED (chỉ dùng nếu bạn cho phép provider tự đổi)
        public string? Status { get; set; }
    }
}

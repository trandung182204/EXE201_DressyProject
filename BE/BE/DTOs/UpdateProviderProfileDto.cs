namespace BE.DTOs
{
    public class UpdateProviderProfileDto
    {
        public string? BrandName { get; set; }
        public string? LogoUrl { get; set; }
        public string? Description { get; set; }
        public string? ProviderType { get; set; } // BOTH | SERVICE | OUTFIT (theo constraint bạn set)
        public string? Status { get; set; }       // ACTIVE | SUSPENDED (nếu bạn cho phép provider tự đổi thì giữ, không thì bỏ)
    }
}

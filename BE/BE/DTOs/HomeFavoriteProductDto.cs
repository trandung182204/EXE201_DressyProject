namespace BE.DTOs
{
    public class HomeFavoriteProductDto
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public decimal? MinPricePerDay { get; set; }

        public long? ImageFileId { get; set; }
        public string? ImageUrl { get; set; }
        public decimal? DepositAmount { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
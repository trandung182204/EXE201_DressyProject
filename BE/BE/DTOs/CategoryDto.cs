namespace BE.DTOs
{
    public class CategoryDto
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public long? ParentId { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
    }
}

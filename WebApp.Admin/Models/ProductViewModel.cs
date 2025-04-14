namespace WebApp.Admin.Models
{
    public class ProductViewModel
    {
        public required string Id { get; set; }
        public string? Name { get; set; }
        public DateOnly? CreateAdd { get; set; }
        public string? CategoryId { get; set; }
        public string? CategoryName { get; set; }
    }
}
namespace WebApp.Shared.Responses;

public class ProductResponse
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public DateOnly? CreateAdd { get; set; }
    public string? CategoryId { get; set; }
    public string? CategoryName { get; set; }
}
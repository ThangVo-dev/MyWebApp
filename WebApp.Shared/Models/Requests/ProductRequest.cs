namespace WebApp.Shared.Requests;

public class ProductRequest
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public DateOnly? CreateAdd { get; set; }
    public string? CategoryId { get; set; }
}
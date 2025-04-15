namespace WebApp.Shared.Models.Requests;

public class EditProfileRequest
{
    public string? Id { get; set; }
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
}
namespace WebApp.Shared.Models.Common
{
    public class ClaimJWTToken
    {
        public string? Id { get; set; }
        public string? Username { get; set; }
        public string? Role { get; set; }
        public string? Issuer { get; set; }
        public string? Audience { get; set; }
        public string? SigningKey { get; set; }
        public int? ExpirationMinutes { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
    }
}
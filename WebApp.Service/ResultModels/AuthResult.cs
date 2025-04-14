using WebApp.Service.Enums;

namespace WebApp.Service.ResultModels
{
    public class ApiResult<T>
    {
        public ApiStatusCode StatusCode { get; set; }
        public string? Token { get; set; }
        public DateTime? Expiration { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; } 
    }
}
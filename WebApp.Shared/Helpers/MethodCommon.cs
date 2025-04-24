using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;

namespace WebApp.Shared.Helpers
{
    public class MethodCommon
    {
        public string ComputeFileHash(IFormFile file)
        {
            using (var md5 = MD5.Create())
            using (var stream = file.OpenReadStream())
            {
                var hashBytes = md5.ComputeHash(stream);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
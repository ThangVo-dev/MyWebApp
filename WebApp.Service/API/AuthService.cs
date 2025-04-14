using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApp.Service.Enums;
using WebApp.Service.ResultModels;

namespace AppWeb.Service.API
{
    public class AuthService
    {
        private readonly IConfiguration _configuration;

        public AuthService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public ApiResult<object> Login(string? username, string? password)
        {
            var authResult = new ApiResult<object>();
            if (username == "admin" && password == "12345")
            {
                var jwtSettings = _configuration.GetSection("JwtSettings");
                var issuer = jwtSettings["Issuer"];
                var audience = jwtSettings["Audience"];
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["IssuerSigningKey"]));

                var claims = new[]
                {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, "Admin")
            };

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddHours(1),
                    Issuer = issuer,
                    Audience = audience,
                    SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);

                authResult.StatusCode = ApiStatusCode.OK;
                authResult.Token = tokenHandler.WriteToken(token);
                authResult.Expiration = tokenDescriptor.Expires;
                return authResult;
            }

            authResult.StatusCode = ApiStatusCode.Unauthorized;
            authResult.Message = "Invalid username or password";
            return authResult;
        }
    }
}
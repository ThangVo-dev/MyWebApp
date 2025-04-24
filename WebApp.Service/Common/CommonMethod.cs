using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using WebApp.Service.Enums;
using WebApp.Service.ResultModels;
using WebApp.Shared.Models.Common;

namespace WebApp.Service.Common
{
    public class CommonMethod
    {
        public string GenerateJwtToken(ClaimJWTToken claimJWTToken)
        {
            if (claimJWTToken == null)
            {
                return string.Empty;
            }

            var issuer = claimJWTToken.Issuer;
            var audience = claimJWTToken.Audience;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(claimJWTToken.SigningKey ?? string.Empty));

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Jti, claimJWTToken.Id ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Sub, claimJWTToken.Username ?? string.Empty),
                new Claim(ClaimTypes.Role, claimJWTToken.Role ?? string.Empty),
                new Claim("FullName", claimJWTToken.FullName ?? string.Empty),
                new Claim("PhoneNumber", claimJWTToken.PhoneNumber ?? string.Empty),
                new Claim("Email", claimJWTToken.Email ?? string.Empty)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(claimJWTToken.ExpirationMinutes ?? 60),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            var jwtToken = tokenHandler.WriteToken(token);
            return jwtToken;
        }

        public IActionResult HandleApiResultAsync<T>(ApiResult<T> apiResultTask)
        {
            var apiResult = apiResultTask;
            switch (apiResult.StatusCode)
            {
                case ApiStatusCode.OK:
                    return new OkObjectResult(new
                    {
                        Success = true,
                        Message = apiResult.Message ?? "Request successful.",
                        Data = apiResult.Data
                    });

                case ApiStatusCode.NotFound:
                    return new NotFoundObjectResult(new
                    {
                        Success = false,
                        Message = apiResult.Message ?? "Resource not found."
                    });

                case ApiStatusCode.BadRequest:
                    return new BadRequestObjectResult(new
                    {
                        Success = false,
                        Message = apiResult.Message ?? "Invalid request."
                    });

                case ApiStatusCode.Unauthorized:
                    return new UnauthorizedResult();

                case ApiStatusCode.Forbidden:
                    return new ForbidResult();

                default:
                    return new ObjectResult(new
                    {
                        Success = false,
                        Message = apiResult.Message ?? "An unknown error occurred."
                    })
                    {
                        StatusCode = (int)apiResult.StatusCode,
                    };
            }

        }
    }
}
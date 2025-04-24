using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WebApp.Data.Entities;
using WebApp.Service.Common;
using WebApp.Service.Enums;
using WebApp.Service.ResultModels;
using WebApp.Shared.Models.Common;
using WebApp.Shared.Models.Requests;
using WebApp.Shared.Responses;

namespace AppWeb.Service.API
{
    public class AuthService
    {
        private readonly IConfiguration _configuration;
        private readonly WebAppContext _context;
        private readonly CommonMethod _commonMethod;

        public AuthService(IConfiguration configuration, WebAppContext context, CommonMethod commonMethod)
        {
            _context = context;
            _configuration = configuration;
            _commonMethod = commonMethod;
        }

        public async Task<IActionResult> LoginAsync(LoginRequest loginRequest)
        {
            var action = new Func<Task<IActionResult>>(async () =>
            {
                var authResult = new ApiResult<TokenResponse>();
                if (loginRequest != null && loginRequest.Username != null && loginRequest.Password != null)
                {
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == loginRequest.Username && u.PasswordHash == loginRequest.Password);
                    if (user == null)
                    {
                        authResult.StatusCode = ApiStatusCode.Unauthorized;
                        authResult.Message = "Invalid username or password";
                    }
                    else
                    {
                        var claimJWTToken = new ClaimJWTToken
                        {
                            Id = user.Id,
                            Username = loginRequest.Username,
                            Role = user.Role?.Name,
                            Issuer = _configuration["JwtSettings:Issuer"],
                            Audience = _configuration["JwtSettings:Audience"],
                            SigningKey = _configuration["JwtSettings:IssuerSigningKey"],
                            ExpirationMinutes = 30,
                            FullName = user.FullName,
                            PhoneNumber = user.PhoneNumber,
                            Email = user.Email
                        };

                        authResult.StatusCode = ApiStatusCode.OK;
                        authResult.Expiration = DateTime.UtcNow.AddMinutes(claimJWTToken.ExpirationMinutes ?? 60);
                        authResult.Token = _commonMethod.GenerateJwtToken(claimJWTToken);
                        authResult.Message = "Login successful";
                        authResult.Data = new TokenResponse
                        {
                            Token = authResult.Token,
                            Expiration = authResult.Expiration
                        };
                    }
                }
                else
                {


                    authResult.StatusCode = ApiStatusCode.Unauthorized;
                    authResult.Message = "Invalid username or password";
                }
                var result = _commonMethod.HandleApiResultAsync<TokenResponse>(authResult);
                return result;
            });

            return await ErrorHandler.ExecuteAsync(action);
        }
    }
}
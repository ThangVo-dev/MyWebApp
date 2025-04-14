using AppWeb.Service.API;
using Microsoft.AspNetCore.Mvc;
using WebApp.Service.Enums;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private AuthService _authService;

    public AuthController(IConfiguration configuration, AuthService authService)
    {
        _authService = authService;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginModel login)
    {
        var result = _authService.Login(login.Username, login.Password);

        if (result.StatusCode == ApiStatusCode.OK)
        {
            return Ok(new
            {
                Token = result.Token,
                Expiration = result.Expiration
            });
        }

        return Unauthorized(new { Message = result.Message });
    }
}

public class LoginModel
{
    public string? Username { get; set; }
    public string? Password { get; set; }
}